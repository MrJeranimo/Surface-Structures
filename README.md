# Surface Structures

This is a KSA mod that will place .glb meshes onto the surface of a Celestial at a specified Landmark. It currently only works with hardcoded values in `LandmarkMeshConfig.cs` but this will change.

## Creating your own Surface Structures

This is currently not possible but will be added in future updates.

## Usage

To use the mod, compile the project and put it in a folder called `Surface Structures/`, make sure to include `ModMenu.Attributes.dll` and the `mod.toml`. Then put that folder into the `My Games/Kitten Space Agency/mods/` folder. 

FInally, add the code below to the `manifest.toml` file in `My Games/Kitten Space Agency/`.

```toml
[[mods]]
id = "Surface Structures"
enabled = true
```
