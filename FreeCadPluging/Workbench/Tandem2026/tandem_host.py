"""Bridge from FreeCAD Python to the C# FreeCadPluging host."""

import os
import subprocess


def _host_path():
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.join(here, "Host", "FreeCadPluging.exe")


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