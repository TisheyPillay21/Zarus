# Zarus

Made with Unity `6000.2.10f1`

## South Africa Region Map Overlay

- The interactive provincial map lives in `Assets/Scripts/Map` and is driven by the `RegionMapController` + `RegionMapCameraController` components inside `Assets/Scenes/Main.unity`.
- All GIS data is sourced from SimpleMaps (see `Assets/Sprites/za.json`). A pre-built `RegionDatabase` asset is generated under `Assets/Resources/Map` the first time the editor compiles scripts. Rebuild it at any time via `Zarus/Map/Rebuild Region Assets`.
- Meshes for each province are stored in `Assets/Map/Meshes`. Artists can tweak colors, descriptions, and URLs per region inside the database asset.
- Licensing/attribution requirements for the SimpleMaps dataset are documented in `Assets/Documentation/ThirdPartyLicenses/SimpleMaps_SouthAfrica.txt` and must ship with the game.
