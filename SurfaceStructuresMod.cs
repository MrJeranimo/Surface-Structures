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
        public static bool Debug = false;
        public static bool _showEditorWindow = false;

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

            ConfigReader config = new ConfigReader("config.json");

            bool debug = config.GetOrDefault<bool>("debug", false);
            Debug = debug;

            if (Debug)
                Console.WriteLine("Surface Structures - Running in debug mode");
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
                            DefaultCategory.Log.Info($"Surface Structures - Found mesh '{structure.MeshID}' for landmark '{landmark.Id}'");
                            LandmarkRenderableRegistry.Add(new LandmarkMeshRenderer(landmark, celestial, structure));
                            structure.SetLoaded();
                        }
                    }
                }
            }

            if (Debug)
            {
                StructureEditor.CreateStructureNames();
            }
        }

        [StarMapAfterGui]
        public void AfterGui(double dt)
        {
            if (StructureEditor.ShowEditorWindow)
            {
                StructureEditor.DrawWindow();
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
            if (!Debug)
            {
                ImGui.Text("Nothing to see here.");
            }
            else
            {
                ImGui.MenuItem("Structure Editor", "", ref StructureEditor.ShowEditorWindow);
            }
        }
    }
}
