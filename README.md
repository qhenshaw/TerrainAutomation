# Terrain Automation
[![Unity 2023.1+](https://img.shields.io/badge/unity-2023.1%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](LICENSE.md)

Rapidly iterate on terrain designs by procedurally placing trees and details based on height and splat maps generated through apps like [Gaea.](https://quadspinner.com/)  

![Unity_ui97O12gsB](https://github.com/qhenshaw/TerrainAutomation/assets/911416/37da9d2d-b291-4e5f-885c-acb6e51c6668)

## Features
### Unified Control
Control important terrain properties through a single component - apply height/splat maps, configure size, and assign layers, trees, and details.  
Configuration is saved in a profile asset that can be shared between terrains/scenes.  

https://github.com/qhenshaw/TerrainAutomation/assets/911416/df6da7a4-c581-4c08-a478-35d49e5e280b

### One-Touch Generation
Once your profile is configured any terrain features can be regenerated with a single click.  

https://github.com/qhenshaw/TerrainAutomation/assets/911416/b245a47f-95e1-4c6f-9a0a-8cb4f3e2879a

### Expanded Options
Additional options are included for procedurally placing trees and details - placement can be affected by splat values, world height, and terrain slope.

![Unity_2gQaRqh08k](https://github.com/qhenshaw/TerrainAutomation/assets/911416/1ec6b106-d9e0-40bd-9adf-18f742088a72)
![Unity_JAbwIjZaiW](https://github.com/qhenshaw/TerrainAutomation/assets/911416/f5440a74-c29d-48ff-b6f0-923e53fc3c77)

### Full Compatibility
The automation system doesn't bypass any built-in terrain functionality, once you're done generating the terrain all the existing sculpting, painting, and placing tools are still available to you.

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
[Usage instructions here]
