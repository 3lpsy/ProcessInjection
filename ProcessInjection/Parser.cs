using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace ProcessInjection
{
  class Parser
  {
    public static Dictionary<string, string> ParseArgs(string[] args, string[] keys)
    {
      var arguments = new Dictionary<string, string>();
      string allargs = String.Join(" ", args);
      string oldkey = String.Empty;
      string currkey = String.Empty;
      string currval = String.Empty;

      for (int i = 0; i < allargs.Length; i++) {
        if (i == allargs.Length - 1 && currkey.Length > 0 && !arguments.ContainsKey(currkey)) {
          currval = currval + allargs[i];
          arguments[currkey] = currval;
        } else if (allargs[i].ToString() == "=" && HasWalkedBackKey(i, allargs, keys)) {
          oldkey = currkey;
          currkey = GetWalkedBackKey(i, allargs, keys);
          //  Save previous if exists
          if (oldkey.Length > 0) {
            char[] charsToTrim = { ' ' };
            arguments[oldkey] = ChopEnd(currval, currkey).TrimEnd(charsToTrim);
          }
          currval = String.Empty;
        } else {
          currval = currval + allargs[i];
        }
      }
    }

    public static bool IsOnlyDigits(string pid)
    {
      bool hasAlpha = false;
      foreach (char c in pid.ToCharArray()) {
        if (!c.IsDigit()) {
          hasAlpha = true;
        }
      }
      return !hasAlpha;
    }



    public static bool ContainsAll(Dictionary<string, string> arguments, string[] keys)
    {
      string key;
      for (int i = 0; i < keys.Length; i++) {
        key = keys[i];
        if (!arguments.ContainsKey(key)) {
          return false;
        }
      }
      return true;
    }


    public static string ChopEnd(string source, string value)
    {
      if (!source.EndsWith(value))
        return source;

      return source.Remove(source.LastIndexOf(value));
    }

    public static bool HasWalkedBackKey(int i, string allargs, string[] keys)
    {
      return GetWalkedBackKey(i, allargs, keys).Length > 0;
    }

    public static string GetWalkedBackKey(int i, string allargs, string[] keys)
    {
      int j = i - 1;
      string curr = String.Empty;
      string rcandidate = String.Empty;
      string candidate = String.Empty;

      if (allargs.Length > 0 && j >= 0) {
        while (j >= 0 && curr != " ") {
          curr = allargs[j].ToString();
          rcandidate = rcandidate + curr;
          j = j - 1;
        }
        char[] charsToTrim = { ' ' };
        rcandidate = rcandidate.TrimEnd(charsToTrim);
        candidate = ReverseString(rcandidate);
        int pos = Array.IndexOf(keys, candidate);
        if (pos > -1) {
          return candidate;
        }
      }
      return String.Empty;
    }

    public static void DebugArgs(Dictionary<string, string> arguments, string[] keys)
    {
      string key;
      string val;
      for (int i = 0; i < keys.Length; i++) {
        key = keys[i];
        if (arguments.ContainsKey(key)) {
          val = arguments[key];
          Console.WriteLine($"[+] Argument {key}: {val}");
        }
      }
    }
  }
}