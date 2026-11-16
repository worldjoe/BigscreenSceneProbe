# Bigscreen Scene Probe

Diagnostic MelonLoader mod for Bigscreen PC VR. Press F8 to save an inventory
of active Unity cameras, streamer cameras, and renderers. It does not enable
passthrough or modify scene rendering.
This is an unofficial community mod, not affiliated with Bigscreen, Inc.

## Requirements

- Windows, Bigscreen PC VR, and an IL2CPP-compatible MelonLoader installation.
- Launch Bigscreen once with MelonLoader to generate
  `MelonLoader\Il2CppAssemblies`.
- .NET 6 SDK or a later SDK capable of targeting .NET 6, for building.

Developed against Bigscreen `0.950.2.03f010-beta-perma-rooms-beta`,
MelonLoader `0.7.1` open beta, and Unity `2020.3.33f1`. Other versions may
require changes.

## Build

From this directory in PowerShell:

```powershell
dotnet build .\BigscreenSceneProbe.csproj -c Release
```

The project expects Bigscreen at
`C:\Program Files (x86)\Steam\steamapps\common\Bigscreen`. For another
installation, supply its directory:

```powershell
dotnet build .\BigscreenSceneProbe.csproj -c Release "-p:GameDir=D:\SteamLibrary\steamapps\common\Bigscreen"
```

Output: `bin\Release\net6.0\BigscreenSceneProbe.dll`. The project references
assemblies from that Bigscreen installation; it does not bundle them.

## Install and use

1. Close Bigscreen.
2. Copy `BigscreenSceneProbe.dll` into Bigscreen's `Mods` folder.
3. Start Bigscreen, enter the scene to inspect, and focus the PC game window.
4. Press **F8**. The mod writes
   `UserData\BigscreenSceneProbe.txt` under the Bigscreen install directory.

Each capture replaces the previous file. The inventory may contain
user-identifying object names and scene details: review it before sharing.
Capture failures are reported in `MelonLoader\Latest.log`. Remove the DLL
from `Mods` while Bigscreen is closed to uninstall; the capture remains in
`UserData` until removed separately.

When the monitor adjustment page is open, F8 also writes
`UserData\BigscreenMonitorProbe-1.txt`, `-2.txt`, or `-3.txt`, according to
the selected monitor. These files contain monitor layout, controls, and local
monitor state; review their paths and labels for identifying information before
sharing. Each capture replaces the file for that monitor.
It also writes a separate file suffixed with the selected 3D format, to compare
material and shader state between formats.
