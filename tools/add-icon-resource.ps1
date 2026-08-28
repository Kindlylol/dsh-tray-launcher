[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ExecutablePath,
    [Parameter(Mandatory)]
    [string]$IconPath
)

$ErrorActionPreference = 'Stop'

$source = @'
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

public static class IconResourceUpdater
{
    private const int RtIcon = 3;
    private const int RtGroupIcon = 14;
    private const ushort AlternateGroupId = 50000;
    private const ushort FirstImageId = 50001;
    private const uint LoadLibraryAsDatafile = 0x00000002;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr BeginUpdateResource(string fileName, bool deleteExistingResources);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool UpdateResource(
        IntPtr update,
        IntPtr type,
        IntPtr name,
        ushort language,
        byte[] data,
        uint dataSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool EndUpdateResource(IntPtr update, bool discard);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryEx(string fileName, IntPtr file, uint flags);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr FindResource(IntPtr module, IntPtr name, IntPtr type);

    [DllImport("kernel32.dll")]
    private static extern bool FreeLibrary(IntPtr module);

    public static void AddAlternateIcon(string executablePath, string iconPath)
    {
        byte[] ico = File.ReadAllBytes(iconPath);
        if (ico.Length < 22 || ReadUInt16(ico, 0) != 0 || ReadUInt16(ico, 2) != 1)
            throw new InvalidDataException("The alternate icon is not a valid ICO file.");

        ushort count = ReadUInt16(ico, 4);
        if (count == 0 || ico.Length < 6 + count * 16)
            throw new InvalidDataException("The alternate icon has no valid frames.");

        IntPtr update = BeginUpdateResource(executablePath, false);
        if (update == IntPtr.Zero) ThrowLastWin32("BeginUpdateResource");

        bool committed = false;
        try
        {
            using (var groupStream = new MemoryStream())
            using (var group = new BinaryWriter(groupStream))
            {
                group.Write((ushort)0);
                group.Write((ushort)1);
                group.Write(count);

                for (int index = 0; index < count; index++)
                {
                    int entry = 6 + index * 16;
                    uint length = ReadUInt32(ico, entry + 8);
                    uint offset = ReadUInt32(ico, entry + 12);
                    if (offset > ico.Length || length > ico.Length - offset)
                        throw new InvalidDataException("The alternate icon contains an invalid frame offset.");

                    byte[] frame = new byte[length];
                    Buffer.BlockCopy(ico, (int)offset, frame, 0, (int)length);
                    ushort imageId = (ushort)(FirstImageId + index);
                    Update(update, RtIcon, imageId, frame);

                    group.Write(ico[entry]);
                    group.Write(ico[entry + 1]);
                    group.Write(ico[entry + 2]);
                    group.Write(ico[entry + 3]);
                    group.Write(ReadUInt16(ico, entry + 4));
                    group.Write(ReadUInt16(ico, entry + 6));
                    group.Write(length);
                    group.Write(imageId);
                }

                Update(update, RtGroupIcon, AlternateGroupId, groupStream.ToArray());
            }

            if (!EndUpdateResource(update, false)) ThrowLastWin32("EndUpdateResource");
            committed = true;
        }
        finally
        {
            if (!committed) EndUpdateResource(update, true);
        }

        IntPtr module = LoadLibraryEx(executablePath, IntPtr.Zero, LoadLibraryAsDatafile);
        if (module == IntPtr.Zero) ThrowLastWin32("LoadLibraryEx");
        try
        {
            if (FindResource(module, (IntPtr)AlternateGroupId, (IntPtr)RtGroupIcon) == IntPtr.Zero)
                ThrowLastWin32("FindResource");
        }
        finally
        {
            FreeLibrary(module);
        }
    }

    private static void Update(IntPtr update, int type, ushort name, byte[] data)
    {
        if (!UpdateResource(update, (IntPtr)type, (IntPtr)name, 0, data, (uint)data.Length))
            ThrowLastWin32("UpdateResource");
    }

    private static ushort ReadUInt16(byte[] data, int offset)
    {
        return (ushort)(data[offset] | data[offset + 1] << 8);
    }

    private static uint ReadUInt32(byte[] data, int offset)
    {
        return (uint)(data[offset]
            | data[offset + 1] << 8
            | data[offset + 2] << 16
            | data[offset + 3] << 24);
    }

    private static void ThrowLastWin32(string operation)
    {
        throw new Win32Exception(Marshal.GetLastWin32Error(), operation + " failed");
    }
}
'@

if (-not ('IconResourceUpdater' -as [type])) {
    Add-Type -TypeDefinition $source -Language CSharp
}

$exe = (Resolve-Path -LiteralPath $ExecutablePath).Path
$ico = (Resolve-Path -LiteralPath $IconPath).Path
[IconResourceUpdater]::AddAlternateIcon($exe, $ico)
Write-Output "Embedded alternate icon group 50000: $ico"
