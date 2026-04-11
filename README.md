# Surface Structures

This is a KSA mod that will place .glb meshes onto the surface of a Celestial at a specified Landmark. The mod will automatically find all Surface Structures defined in other mods in `Documents/My Games/Kitten Space Agency/mods` that have a file named `Surface Structures.xml`.

This mod does not include any Surface Structures on its own. You must either make your own Structures or download a mod that has them.

> [!Important]
> Requirements:
> - [StarMap mod loader](https://github.com/StarMapLoader/StarMap)
> - [ModMenu](https://spacedock.info/mod/4054/ModMenu)

**License:** MIT

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
<Structures>
    <SurfaceStructure id="Direction Cube">
        <Landmark name="CCSFS LC-39A"/>
        <MeshID id="DirectionCube"/>
        <Position x="0" y="0" z="0"/>
        <Rotation x="0" y="0" z="0"/>
        <Scale x="1" y="1" z="1"/>
    </SurfaceStructure>

    <SurfaceStructure id="Direction Cube 2">
        <Landmark name="CCSFS LC-39A"/>
        <MeshID id="DirectionCube"/>
        <Position x="0" y="0" z="0"/>
        <Rotation x="0" y="0" z="0"/>
        <Scale x="1" y="1" z="1"/>
    </SurfaceStructure>
</Structures>
```

> [!NOTE]
> The `<Position>`, `<Rotation>`, and `<Scale>` tags currently do not work. They are only there for the future, when fixed.

The `id` in `<SurfaceStructure id="">` is the name of the structure.

The `name` in `<Landmark name="CCSFS LC-39A"/>` is the name of the landmark you want to place the mesh at. The landmark must be defined in the planet's or Astronomical's XML definition in `Program Files/Kitten Space Agency/Content/Core`. 

For example,
```xml
<Landmark Id="CCSFS LC-39A">
  <Latitude Degrees="28.60829876577433" />
  <Longitude Degrees="-80.60412690984597" />
</Landmark>
```

The `id` in `<MeshID id="DirectionCube"/>` is the defined `id` in a `<GltfFile id="">` that must be defined in an XML file in `Program Files/Kitten Space Agency/Content/Core`.

> [!IMPORTANT]
> `<SurfaceStructure id="">`, `<Landmark name=""/>`, and `<MeshID id=""/>` must all exists and have valid values to register the Mesh to be drawn at the Landmark.

Each `<Surface Structure>` set of tags you create will correspond to one new Structure/Mesh drawn by this mod. Once you have your `Surface Structures.xml` filled out, the Meshes should be automatically drawn to those landmarks.
