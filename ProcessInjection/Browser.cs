using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using System.Threading;
using System.Security.Permissions;
using Microsoft.Win32;
namespace ProcessInjection
{

  class Browser
  {
    public static string CHOICE_KEY = "Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\http\\UserChoice";

    public static int FindBrowserPid()
    {
      Browsers def = GetDefaultBrowser();
      if (def == Browsers.Unknown || def == Browsers.Safari) {
        def = GetDefaultHTTP();
      }
      int pid;

      pid = FindPidForBrowser(def);
      if (pid > 0) {
        return pid;
      }

      if (def != Browsers.Chrome) {
        Console.WriteLine("[!] Default browser pid not found. Checking chrome anyways");
        pid = FindPidForBrowser(Browsers.Chrome);
        if (pid > 0) {
          return pid;
        }
      }

      if (def != Browsers.InternetExplorer) {
        Console.WriteLine("[!] Default browser pid not found. Checking InternetExplorer anyways");
        pid = FindPidForBrowser(Browsers.InternetExplorer);
        if (pid > 0) {
          return pid;
        }
      }
      Console.WriteLine("[!] Could not find default browser pid");
      return 0;
    }

    public static int FindPidForBrowser(Browsers b)
    {
      string[] checks;

      if (b != Browsers.Unknown && b != Browsers.Safari) {
        if (b == Browsers.InternetExplorer) {
          checks = new string[] { "iexplorer", "Internet Explorer" };
          return Resolver.FindFirstPidByName(checks);
        } else if (b == Browsers.Chrome) {
          checks = new string[] { "Chrome", "chrome", "chromium", "GoogleUpdate", "gupdate", "gupdatem" };
          return Resolver.FindFirstPidByName(checks);
        } else if (b == Browsers.Firefox) {
          checks = new string[] { "Firefox", "firefox", "waterfox" };
          return Resolver.FindFirstPidByName(checks);
        } else if (b == Browsers.Edge) {
          checks = new string[] { "Edge", "edge", "MicrosoftEdge", "MicrosoftEdgeCP" };
          return Resolver.FindFirstPidByName(checks);
        }
      }
      Console.WriteLine("[!] Unknown Browsers.");
      return 0;
    }

    public enum Browsers
    {
      Unknown,
      InternetExplorer,
      Firefox,
      Chrome,
      Opera,
      Safari,
      Edge

    }


    public static Browsers GetDefaultBrowser()
    {
      Browsers browser;
      string progId;
      using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(CHOICE_KEY)) {
        if (userChoiceKey == null) {
          Console.WriteLine("[!] No user choice for browser in registry.");
          return Browsers.Unknown;
        }
        object progIdValue = userChoiceKey.GetValue("Progid");
        if (progIdValue == null) {
          Console.WriteLine("[!] No progid for browser in registry.");
          return Browsers.Unknown;
        }
        progId = progIdValue.ToString();
        Console.WriteLine($"[>] Browser Prog ID: {progId}");

        switch (progId) {
          case "IE.HTTP":
            browser = Browsers.InternetExplorer;
            break;
          case "FirefoxURL":
            browser = Browsers.Firefox;
            break;
          case "ChromeHTML":
            browser = Browsers.Chrome;
            break;
          case "OperaStable":
            browser = Browsers.Opera;
            break;
          case "SafariHTML":
            browser = Browsers.Safari;
            break;
          case "AppXq0fevzme2pys62n3e0fbqa7peapykr8v":
            browser = Browsers.Edge;
            break;
          default:
            browser = Browsers.Unknown;
            break;
        }
      }

      return browser;
    }
    public static Browsers GetDefaultHTTP()
    {
      string name = string.Empty;
      RegistryKey regKey = null;
      try {
        //set the registry key we want to open
        regKey = Registry.ClassesRoot.OpenSubKey("HTTP\\shell\\open\\command", false);

        //get rid of the enclosing quotes
        name = regKey.GetValue(null).ToString().ToLower().Replace("" + (char)34, "");

        //check to see if the value ends with .exe (this way we can remove any command line arguments)
        if (!name.EndsWith("exe")) {
          //get rid of all command line arguments (anything after the .exe must go)
          name = name.Substring(0, name.LastIndexOf(".exe") + 4);
        }

        if (name.ToLower().Contains("firefox")) {
          return Browsers.Firefox;
        } else if (name.ToLower().Contains("chrome")) {
          return Browsers.Chrome;
        } else if (name.ToLower().Contains("edge")) {
          return Browsers.Edge;
        } else if (name.ToLower().Contains("safari")) {
          return Browsers.Safari;
        } else if (name.ToLower().Contains("iexplorer") || name.ToLower().StartsWith("ie")) {
          return Browsers.InternetExplorer;
        } else if (name.ToLower().Contains("opera")) {
          return Browsers.Opera;
        }
        Console.WriteLine($"[!] Unknown browser open value: {name}");
        return Browsers.Unknown;


      } catch (Exception ex) {
        Console.WriteLine("[!] Error reading browser open registry");
      } finally {
        //check and see if the key is still open, if so
        //then close it
        if (regKey != null)
          regKey.Close();
      }
      //return the value
      return Browsers.Unknown;

    }
  }
}