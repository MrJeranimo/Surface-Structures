using Brutal.ImGuiApi;
using Brutal.Logging;
using Brutal.Numerics;
using HarmonyLib;
using KSA;
using ModMenu;
using StarMap.API;

namespace Surface_Structures
{
    [StarMapMod]
    public class SurfaceStructuresMod
    {
        private static readonly Harmony MHarmony = new Harmony("Surface Structures");
        private static bool _hasRan = false;
        public static float3 Position = new float3(0, 0, 0);

        [StarMapBeforeMain]
        public void BeforeSystemSelect()
        {
            MHarmony.PatchAll(typeof(SurfaceStructuresMod).Assembly);

            XMLStructureFinder.FindModFolder();
            Dictionary<string, string[]> found = XMLStructureFinder.FindSurfaceStructuresFiles();

            foreach (var (modName, filePaths) in found)
            {
                foreach (var filePath in filePaths)
                {
                    Console.WriteLine($"Surface Structures - Found new XML file in, {modName}: {filePath}");
                    SurfaceStructureParser.ParseFile(filePath);
                }
            }

            SurfaceStructureParser.RegisterLandmarkStructures();
        }

        [StarMapBeforeGui]
        public void CelestialSystemLoaded(double dt)
        {
            if (_hasRan || Universe.CurrentSystem == null)
            {
                return;
            }
            _hasRan = true;
            LandmarkRenderableRegistry.Clear();

            foreach (Celestial celestial in Universe.CurrentSystem.All.OfType<Celestial>())
            {
                foreach (var location in celestial.BodyTemplate.Locations)
                {
                    if (location is LandmarkReference landmark && LandmarkRenderableRegistry.MeshMap.TryGetValue(landmark.Id, out var landmarkStructures))
                    {
                        foreach (var structure in landmarkStructures)
                        {
                            DefaultCategory.Log.Info($"LandmarkStructureConfig: Found mesh '{structure.MeshID}' for landmark '{landmark.Id}'");
                            LandmarkRenderableRegistry.Add(new LandmarkMeshRenderer(landmark, celestial, structure));
                        }
                    }
                }
            }
        }

        [StarMapUnload]
        public void Unload()
        {
            MHarmony.UnpatchAll(nameof(SurfaceStructuresMod));
        }

        [ModMenuEntry("Surface Structures")]
        public static void ModMenuEntry()
        {
            ImGui.DragFloat3("Position", ref Position);
        }
    }
}
