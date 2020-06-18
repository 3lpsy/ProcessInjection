using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using System.Threading;

namespace ProcessInjection
{

  class Browser
  {
    public static string CHOICE_KEY = "Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\http\\UserChoice";

    public static int FindBrowserPid()
    {
      Browser def = GetDefaultBrowser();
      if (def == Browser.Unknown || def == Browser.Safari) {
        def = GetDefaultHTTP();
      }
      int pid;

      pid = FindPidForBrowser(def);
      if (pid > 0) {
        return pid;
      }

      if (def != Browser.Chrome) {
        Console.WriteLine("[!] Default browser pid not found. Checking chrome anyways");
        pid = FindPidForBrowser(Browser.Chrome);
        if (pid > 0) {
          return pid;
        }
      }

      if (def != Browser.InternetExplorer) {
        Console.WriteLine("[!] Default browser pid not found. Checking InternetExplorer anyways");
        pid = FindPidForBrowser(Browser.InternetExplorer);
        if (pid > 0) {
          return pid;
        }
      }
      Console.WriteLine("[!] Could not find default browser pid");
      return 0;
    }

    public static int FindPidForBrowser(Browser b)
    {
      string[] checks;

      if (b != Browser.Unknown && b != Browser.Safari) {
        if (b == Browser.InternetExplorer) {
          checks = new string[] { "iexplorer", "Internet Explorer" };
          return Resolver.FindFirstPidByName(checks);
        } else if (b == Browser.Chrome) {
          checks = new string[] { "Chrome", "chrome", "chromium", "GoogleUpdate", "gupdate", "gupdatem" };
          return Resolver.FindFirstPidByName(checks);
        } else if (b == Browser.Firefox) {
          checks = new string[] { "Firefox", "firefox", "waterfox" };
          return Resolver.FindFirstPidByName(checks);
        } else if (b == Browser.Edge) {
          checks = new string[] { "Edge", "edge", "MicrosoftEdge", "MicrosoftEdgeCP" };
          return Resolver.indFirstPidByName(checks);
        }
      }
      Console.WriteLine("[!] Unknown browser.");
      return 0;
    }

    public static enum Browser
    {
      Unknown,
      InternetExplorer,
      Firefox,
      Chrome,
      Opera,
      Safari,
      Edge

    }


    public static Browser GetDefaultBrowser()
    {
      Browser browser;
      using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(CHOICE_KEY)) {
        if (userChoiceKey == null) {
          browser = Browser.Unknown;
          break;
        }
        object progIdValue = userChoiceKey.GetValue("Progid");
        if (progIdValue == null) {
          browser = Browser.Unknown;
          break;
        }
        progId = progIdValue.ToString();
        switch (progId) {
          case "IE.HTTP":
            browser = Browser.InternetExplorer;
            break;
          case "FirefoxURL":
            browser = Browser.Firefox;
            break;
          case "ChromeHTML":
            browser = Browser.Chrome;
            break;
          case "OperaStable":
            browser = Browser.Opera;
            break;
          case "SafariHTML":
            browser = Browser.Safari;
            break;
          case "AppXq0fevzme2pys62n3e0fbqa7peapykr8v":
            browser = Browser.Edge;
            break;
          default:
            browser = Browser.Unknown;
            break;
        }
      }

      return browser;
    }
    public static Browser GetDefaultHTTP()
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
          return Browser.Firefox;
        } else if (name.ToLower().Contains("chrome")) {
          return Browser.Chrome;
        } else if (name.ToLower().Contains("edge")) {
          return Browser.Edge;
        } else if (name.ToLower().Contains("safari")) {
          return Browser.Safari;
        } else if (name.ToLower().Contains("iexplorer") || name.ToLower().StartsWith("ie")) {
          return Browser.InternetExplorer;
        } else if (name.ToLower().Contains("opera")) {
          return Browser.Opera;
        }
        return Browser.Unknown;


      } catch (Exception ex) {
        name = string.Format("ERROR: An exception of type: {0} occurred in method: {1} in the following module: {2}", ex.GetType(), ex.TargetSite, this.GetType());
      } finally {
        //check and see if the key is still open, if so
        //then close it
        if (regKey != null)
          regKey.Close();
      }
      //return the value
      return name;

    }
  }
}