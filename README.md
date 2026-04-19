# Surface Structures

This is a KSA mod that will place .glb meshes onto the surface of a Celestial at a specified Landmark. The mod will automatically find all Surface Structures defined in other mods in `Documents/My Games/Kitten Space Agency/mods` that have a file named `Surface Structures.xml`.

> [!Important]
> Requirements:
> - [StarMap mod loader](https://github.com/StarMapLoader/StarMap)
> - [ModMenu](https://spacedock.info/mod/4054/ModMenu)

**License:** MIT

**SpaceDock:** https://spacedock.info/mod/4202/Surface%20Structures

## Usage

To use the mod, download the latest release and extract the folder into the `My Games/Kitten Space Agency/mods/` folder. 

Finally, add the code below to the `manifest.toml` file in `My Games/Kitten Space Agency/`.

```toml
[[mods]]
id = "Surface Structures"
enabled = true
```

## Creating your own Surface Structure(s)

To create your own Surface Structure(s), you must create a `Surface Structures.xml` file in your mod. It can be at the top-level folder of your mod or below, this mod will find it. Inside the XML file, you should follow this layout for each Structure you wish to add. 

```xml
<?xml version="1.0" encoding="utf-8"?>
<Structures>
  <Landmark Name="Example Landmark">
    <Celestial Id="Earth" />
    <Longitude Degrees="-60.528" />
    <Latitude Degrees="31.997" />
    <Visible Value="true" />
  </Landmark>
  <SurfaceStructure Id="VAB">
    <Landmark Name="CCSFS LC-39A" />
    <PartId Id="VAB" />
    <Position X="-700" Y="0" Z="10" />
    <Rotation X="270" Y="0" Z="0" />
    <Scale X="3" Y="3" Z="3" />
    <Visible Value="true" />
  </SurfaceStructure>
</Structures>
```

Each `<Surface Structure>` set of tags you create will correspond to one new Structure/Mesh drawn by this mod. Once you have your `Surface Structures.xml` filled out, the Meshes should be automatically drawn to those landmarks.

To easily make complicated structures, there is a built-in Structure Editor ONLY when running in `debug` mode. To run the mod in `debug` mode, change the "debug" value to `true` in `config.json`.

### What each tag means

The `Id` in `<SurfaceStructure Id="">` is the name of the structure.

The `Name` in `<Landmark Name="CCSFS LC-39A"/>` is the name of the landmark you want to place the mesh at. The landmark can be defined by KSA Core or inside the `Surface Structures.xml` file.

For example,
```xml
<Landmark Name="Example Landmark">
    <Celestial Id="Earth" />
    <Longitude Degrees="-60.528" />
    <Latitude Degrees="31.997" />
    <Visible Value="true" />
</Landmark>
```

The `Id` in `<PartId Id="DirectionCube"/>` is the defined `Id` in a `<Part Id="">`.

> [!IMPORTANT]
> `<SurfaceStructure Id="">`, `<Landmark Name=""/>`, and `<PartId Id=""/>` must all exists and have valid values to register the Mesh to be drawn at the Landmark.

The `<Visible>` tag just set the mesh to be visible or not. It is an optional tag as, by default, it is set to `true`.

The `<Position>`, `<Rotation>`, and `<Scale>` tags let you apply transfom offsets to the mesh using the Landmark as (0, 0, 0). These tags are optional as, by default, they will be set to `(0, 0, 0)`, `(0, 0, 0)`, and `(1, 1, 1)` respectively. `<Rotation>` uses Degrees for its float values.

> [!NOTE]
> The convention follows the Right-Hand-Rule with,
> 
> +X = Forward [East]
> 
> +Y = Left [North]
> 
> +Z = Up [Up]

This is self explanitory for Position and Scale. 

> [!NOTE]
>
> For Rotation,
>
> +X will PITCH the Foward side UP
>
> +Y will ROLL the mesh to the LEFT
>
> +Z will YAW the mesh to the RIGHT.

The Position, Rotation, and Scale are all independent of each other so you do not need to worry about order of application. The Rotations are done with a Quaternion so you do not need to worry about gimbal lock.
