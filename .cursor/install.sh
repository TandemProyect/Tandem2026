#!/usr/bin/env bash
# =============================================================================
# Tandem 2026 — Cloud Agent install script (idempotent).
#
# This is a .NET Framework 4.8 solution (ASP.NET MVC 5 web app "Desing",
# EF6/EDMX "DAL", trivial "LinkedIn" lib, and a Windows-only ZWCAD/WPF plugin).
# On the Linux Cloud Agent VM we set up the Mono toolchain + Roslyn MSBuild,
# restore NuGet packages, and compile every project that can build off Windows.
#
# The ZwcadPlugin project is intentionally NOT built: it targets WPF and the
# Windows-only ZWCAD 2026 managed API (C:\Program Files\ZWSOFT\ZWCAD 2026),
# neither of which exist on Mono/Linux.
# =============================================================================
set -euo pipefail
export DEBIAN_FRONTEND=noninteractive

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]:-$0}")" && pwd)"
# When run as the environment install command the CWD is the repo root.
if [ -f "Design.sln" ]; then REPO_ROOT="$(pwd)"; fi
cd "$REPO_ROOT"

echo "==> [1/5] System toolchain: Mono runtime"
if ! command -v mono >/dev/null 2>&1; then
  sudo apt-get update -y
  sudo apt-get install -y --no-install-recommends mono-complete ca-certificates gnupg curl
fi

echo "==> [2/5] System toolchain: MSBuild (Roslyn) from Mono repo"
if ! command -v msbuild >/dev/null 2>&1; then
  sudo gpg --homedir /tmp --no-default-keyring \
    --keyring /usr/share/keyrings/mono-official-archive-keyring.gpg \
    --keyserver hkp://keyserver.ubuntu.com:80 \
    --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
  echo "deb [signed-by=/usr/share/keyrings/mono-official-archive-keyring.gpg] https://download.mono-project.com/repo/ubuntu stable-focal main" \
    | sudo tee /etc/apt/sources.list.d/mono-official-stable.list >/dev/null
  sudo apt-get update -y
  sudo apt-get install -y --no-install-recommends msbuild
fi

echo "==> [3/5] NuGet CLI + restore"
NUGET=/opt/nuget/nuget.exe
if [ ! -f "$NUGET" ]; then
  sudo mkdir -p /opt/nuget
  sudo curl -fsSL -o "$NUGET" https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
fi
mono "$NUGET" restore Design.sln -NonInteractive

echo "==> [4/5] Wire Roslyn C# compiler into Mono MSBuild"
# Ubuntu's Mono 6.8 MSBuild has no bundled Roslyn, so csc defaults to an old
# language version and rejects the C# 7+ syntax this codebase uses. Reuse the
# self-contained Roslyn shipped inside the restored DotNetCompilerPlatform
# package so csc supports modern C#.
ROSLYN_SRC="$(ls -d packages/Microsoft.CodeDom.Providers.DotNetCompilerPlatform.*/tools/Roslyn-* 2>/dev/null | head -1)"
if [ -z "$ROSLYN_SRC" ]; then
  echo "ERROR: Roslyn not found under packages/ (did the NuGet restore succeed?)" >&2
  exit 1
fi
sudo mkdir -p /opt/roslyn
sudo cp -a "$ROSLYN_SRC"/. /opt/roslyn/
sudo ln -sfn /opt/roslyn /usr/lib/mono/msbuild/Current/bin/Roslyn

echo "==> [5/5] Build Linux-buildable projects (Debug)"
msbuild /p:Configuration=Debug /v:m DAL/DAL.csproj
msbuild /p:Configuration=Debug /v:m Desing/Design.csproj
msbuild /p:Configuration=Debug /v:m LinkedIn/LinkedIn.csproj

echo "==> Install complete. Built assemblies:"
ls -la DAL/bin/Debug/DAL.dll Desing/bin/Desing.dll LinkedIn/bin/Debug/LinkedIn.dll
