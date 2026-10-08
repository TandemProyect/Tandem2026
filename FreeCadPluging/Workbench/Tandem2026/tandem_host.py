"""Bridge from FreeCAD Python to the C# FreeCadPluging host."""

import os
import subprocess


def _host_path():
    here = os.path.dirname(os.path.abspath(__file__))
    candidates = [
        os.path.join(here, "Host", "FreeCadPluging.exe"),
        os.path.join(os.environ.get("APPDATA", ""), "FreeCAD", "v1-1", "Mod", "Tandem2026", "Host", "FreeCadPluging.exe"),
        os.path.abspath(os.path.join(here, "..", "..", "bin", "Debug", "net8.0-windows", "FreeCadPluging.exe")),
    ]
    for candidate in candidates:
        if os.path.isfile(candidate):
            return candidate
    return candidates[0]


def run_host(*args):
    host = _host_path()
    if not os.path.isfile(host):
        return 1, "", "No existe el host C#: {0}. Compila FreeCadPluging.".format(host)
    completed = subprocess.run(
        [host] + list(args),
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0))
    return completed.returncode, completed.stdout.strip(), completed.stderr.strip()


def run_host_detached(*args):
    host = _host_path()
    if not os.path.isfile(host):
        return False, "No existe el host C#: {0}. Compila FreeCadPluging.".format(host)
    subprocess.Popen(
        [host] + list(args),
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
        stdin=subprocess.DEVNULL,
        creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0))
    return True, ""