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

    }

    public static BrowserApplication GetDefaultBrowser()
    {
      BrowserApplication browser;
      using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(CHOICE_KEY)) {
        if (userChoiceKey == null) {
          browser = BrowserApplication.Unknown;
          break;
        }
        object progIdValue = userChoiceKey.GetValue("Progid");
        if (progIdValue == null) {
          browser = BrowserApplication.Unknown;
          break;
        }
        progId = progIdValue.ToString();
        switch (progId) {
          case "IE.HTTP":
            browser = BrowserApplication.InternetExplorer;
            break;
          case "FirefoxURL":
            browser = BrowserApplication.Firefox;
            break;
          case "ChromeHTML":
            browser = BrowserApplication.Chrome;
            break;
          case "OperaStable":
            browser = BrowserApplication.Opera;
            break;
          case "SafariHTML":
            browser = BrowserApplication.Safari;
            break;
          case "AppXq0fevzme2pys62n3e0fbqa7peapykr8v":
            browser = BrowserApplication.Edge;
            break;
          default:
            browser = BrowserApplication.Unknown;
            break;
        }
      }

    }
  }
}