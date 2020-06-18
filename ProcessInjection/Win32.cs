using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Runner
{
  class Win32
  {
    public enum ProcessAccessRights
    {
      All = 0x001F0FFF,
      Terminate = 0x00000001,
      CreateThread = 0x00000002,
      VirtualMemoryOperation = 0x00000008,
      VirtualMemoryRead = 0x00000010,
      VirtualMemoryWrite = 0x00000020,
      DuplicateHandle = 0x00000040,
      CreateProcess = 0x000000080,
      SetQuota = 0x00000100,
      SetInformation = 0x00000200,
      QueryInformation = 0x00000400,
      QueryLimitedInformation = 0x00001000,
      Synchronize = 0x00100000
    }
    [StructLayout(LayoutKind.Sequential)]
    //internal struct STARTUPINFO
    public struct STARTUPINFO
    {
      uint cb;
      IntPtr lpReserved;
      IntPtr lpDesktop;
      IntPtr lpTitle;
      uint dwX;
      uint dwY;
      uint dwXSize;
      uint dwYSize;
      uint dwXCountChars;
      uint dwYCountChars;
      uint dwFillAttributes;
      public uint dwFlags;
      public ushort wShowWindow;
      ushort cbReserved;
      IntPtr lpReserved2;
      IntPtr hStdInput;
      IntPtr hStdOutput;
      IntPtr hStdErr;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct STARTUPINFOEX
    {
      public STARTUPINFO StartupInfo;
      public IntPtr lpAttributeList;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
      public IntPtr hProcess;
      public IntPtr hThread;
      public int dwProcessId;
      public int dwThreadId;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
      public int nLength;
      public IntPtr lpSecurityDescriptor;
      [MarshalAs(UnmanagedType.Bool)]
      public bool bInheritHandle;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(
        ProcessAccessFlags processAccess,
        bool bInheritHandle,
        int processId
    );

    [DllImport("Kernel32", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

    [DllImport("Kernel32", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern bool CreateProcess(IntPtr lpApplicationName, string lpCommandLine, IntPtr lpProcAttribs, IntPtr lpThreadAttribs, bool bInheritHandles, uint dwCreateFlags, IntPtr lpEnvironment, IntPtr lpCurrentDir, [In] ref STARTUPINFO lpStartinfo, out PROCESS_INFORMATION lpProcInformation);

    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

    [Flags]
    public enum ProcessAccessFlags : uint
    {
      All = 0x001F0FFF,
      Terminate = 0x00000001,
      CreateThread = 0x00000002,
      VirtualMemoryOperation = 0x00000008,
      VirtualMemoryRead = 0x00000010,
      VirtualMemoryWrite = 0x00000020,
      DuplicateHandle = 0x00000040,
      CreateProcess = 0x000000080,
      SetQuota = 0x00000100,
      SetInformation = 0x00000200,
      QueryInformation = 0x00000400,
      QueryLimitedInformation = 0x00001000,
      Synchronize = 0x00100000
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        uint nSize,
        out UIntPtr lpNumberOfBytesWritten
    );

    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAllocEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        uint dwSize,
        AllocationType flAllocationType,
        MemoryProtection flProtect
    );

    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateRemoteThread(
        IntPtr hProcess,
        IntPtr lpThreadAttributes,
        uint dwStackSize,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        uint dwCreationFlags,
        out IntPtr lpThreadId
    );

    [Flags]
    public enum AllocationType
    {
      Commit = 0x1000,
      Reserve = 0x2000,
      Decommit = 0x4000,
      Release = 0x8000,
      Reset = 0x80000,
      Physical = 0x400000,
      TopDown = 0x100000,
      WriteWatch = 0x200000,
      LargePages = 0x20000000
    }

    [Flags]
    public enum MemoryProtection
    {
      Execute = 0x10,
      ExecuteRead = 0x20,
      ExecuteReadWrite = 0x40,
      ExecuteWriteCopy = 0x80,
      NoAccess = 0x01,
      ReadOnly = 0x02,
      ReadWrite = 0x04,
      WriteCopy = 0x08,
      GuardModifierflag = 0x100,
      NoCacheModifierflag = 0x200,
      WriteCombineModifierflag = 0x400
    }

  }
}