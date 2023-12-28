# Terrain Automation
[![Unity 2023.1+](https://img.shields.io/badge/unity-2023.1%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](LICENSE.md)

Rapidly iterate on terrain designs by procedurally placing trees and details based on height and splat maps generated through apps like [Gaea.](https://quadspinner.com/)  
<img align="center" src="https://github.com/qhenshaw/TerrainAutomation/assets/911416/37da9d2d-b291-4e5f-885c-acb6e51c6668">  

## Features
### Unified Control
Control important terrain properties through a single component - apply height/splat maps, configure size, and assign layers, trees, and details. Configuration is saved in a profile asset that can be shared between terrains/scenes.  
<div align="center">
  <img align="top" src="https://github.com/qhenshaw/TerrainAutomation/assets/911416/9508c891-896f-4748-a4c1-2494a9c3e8aa" width="300" alt="animated">
  <img align="top" src="https://github.com/qhenshaw/TerrainAutomation/assets/911416/1ec6b106-d9e0-40bd-9adf-18f742088a72" width="300">
  <img align="top" src="https://github.com/qhenshaw/TerrainAutomation/assets/911416/f5440a74-c29d-48ff-b6f0-923e53fc3c77" width="300">
</div>

### One-Touch Generation
Once your profile is configured any terrain features can be regenerated with a single click.  
Height and splat data are applied and then used to procedurally place assigned trees and details.
<div align="center">
    <img src="https://github.com/qhenshaw/TerrainAutomation/assets/911416/f4c6ba45-a61e-4b61-ba76-c1e5dd5ced8c" alt="animated">
</div>

### Non-Destructive Workflows
Apply terrain stamps in the form of subtractive or additive heightmaps to add hills, mountains, or gullies.  
Draw splines with the path component to add paths and roads.  
Modifications through this system are non-destructive, the components can be freely moved and the terrain regenerated in seconds to see the results.
<div align="center">
  <img align="top" src="https://github.com/qhenshaw/TerrainAutomation/assets/911416/6fe170c1-e669-489c-b5c0-a4a2e597b282" width="450">
  <img align="top" src="https://github.com/qhenshaw/TerrainAutomation/assets/911416/799649b7-ec63-4982-b9d9-4b648257b18f" width="450">
</div>

### Terrain System Compatibility
The automation system doesn't bypass any built-in terrain functionality or require specific shaders, once you're done generating the terrain all the existing sculpting, painting, and placing tools are still available to you.

### Example Shaders
Included in the package are example HDRP shaders for sampling the terrain color and applying wind vertex animation to trees and foliage.

## System Requirements
Unity 2023.1+. Will likely work on earlier versions but this is the version I tested with.  
Requires the Splines and Mathematics Unity packages (will install automatically).  

> [!Caution]
> Requires [Odin Inspector](https://odininspector.com/) for custom editors I wrote. There is no plan to move this package away from Odin.

I strongly recommend using a version with [APV (probe volume)](https://portal.productboard.com/unity/1-unity-platform-rendering-visual-effects/c/2047-adaptive-probe-volumes-apv-out-of-experimental) lighting available for lighting large terrains.  

## Installation
Use the Package Manager and use Add package from git URL, using the following: 
```
https://github.com/qhenshaw/TerrainAutomation.git)https://github.com/qhenshaw/TerrainAutomation.git
```

## Usage
Coming soon!
