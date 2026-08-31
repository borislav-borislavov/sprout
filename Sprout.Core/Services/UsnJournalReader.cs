using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Sprout.Core.Services;

/// <summary>
/// Provides read-only access to NTFS USN Journal data for a single file,
/// allowing callers to detect whether a file has changed without reading its contents.
///
/// Each call to <see cref="GetUsnRecord"/> opens the file, queries its
/// current USN record via FSCTL_READ_FILE_USN_DATA, and returns the file's
/// USN (change counter), FileReferenceNumber (volume-unique identity), and
/// Reason (bitmask describing the last change type).
///
/// Callers should persist the returned USN and compare it against a later
/// read: an unchanged USN means the file is untouched; a different USN
/// means it changed. FileReferenceNumber should be compared alongside it
/// to confirm the path still refers to the same underlying file (it only
/// changes if the file was deleted and replaced).
///
/// Requires an NTFS or ReFS volume, and typically elevated privileges since
/// the file is opened with FILE_FLAG_BACKUP_SEMANTICS. Not thread-safe;
/// each instance owns a single native file handle released via Dispose.
/// </summary>
public sealed class UsnJournalReader : IDisposable
{
    private const uint FSCTL_READ_FILE_USN_DATA = 0x900EB;
    private const uint GENERIC_READ = 0x80000000;
    private const uint FILE_SHARE_READ = 0x1;
    private const uint FILE_SHARE_WRITE = 0x2;
    private const uint OPEN_EXISTING = 3;
    private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

    [StructLayout(LayoutKind.Sequential)]
    private struct USN_RECORD_V2
    {
        public uint RecordLength;
        public ushort MajorVersion;
        public ushort MinorVersion;
        public ulong FileReferenceNumber;
        public ulong ParentFileReferenceNumber;
        public long Usn;
        public long TimeStamp;
        public uint Reason;
        public uint SourceInfo;
        public uint SecurityId;
        public uint FileAttributes;
        public ushort FileNameLength;
        public ushort FileNameOffset;
        // FileName follows in the buffer at FileNameOffset
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    private IntPtr _fileHandle = IntPtr.Zero;

    /// <summary>
    /// Reads the current USN record for the given file. Requires NTFS
    /// and typically elevated/admin privileges to open the volume/file
    /// with the needed access for this control code.
    /// </summary>
    public (long Usn, ulong FileReferenceNumber, uint Reason) GetUsnRecord(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        _fileHandle = CreateFile(
            filePath,
            GENERIC_READ,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            IntPtr.Zero,
            OPEN_EXISTING,
            FILE_FLAG_BACKUP_SEMANTICS,
            IntPtr.Zero);

        if (_fileHandle == IntPtr.Zero || _fileHandle == new IntPtr(-1))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to open file handle.");

        int bufferSize = 4096;
        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            bool success = DeviceIoControl(
                _fileHandle,
                FSCTL_READ_FILE_USN_DATA,
                IntPtr.Zero, 0,
                buffer, (uint)bufferSize,
                out uint bytesReturned,
                IntPtr.Zero);

            if (!success)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "FSCTL_READ_FILE_USN_DATA failed.");

            var record = Marshal.PtrToStructure<USN_RECORD_V2>(buffer);
            return (record.Usn, record.FileReferenceNumber, record.Reason);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void Dispose()
    {
        if (_fileHandle != IntPtr.Zero && _fileHandle != new IntPtr(-1))
        {
            CloseHandle(_fileHandle);
            _fileHandle = IntPtr.Zero;
        }
    }
}