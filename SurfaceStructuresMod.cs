using Brutal.ImGuiApi;
using Brutal.Logging;
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

        [StarMapBeforeMain]
        public void BeforeSystemSelect()
        {
            MHarmony.PatchAll(typeof(SurfaceStructuresMod).Assembly);
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
                    if (location is LandmarkReference landmark &&
                        LandmarkRenderableRegistry.MeshMap.TryGetValue(landmark.Id, out var landmarkStructure))
                    {
                        DefaultCategory.Log.Info($"LandmarkStructureConfig: Found mesh '{landmarkStructure.MeshID}' for landmark '{landmark.Id}'", "OnSystemLoaded");

                        LandmarkRenderableRegistry.Add(
                            new LandmarkMeshRenderer(landmark, celestial, landmarkStructure));
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
            double temp = LandmarkMeshRenderer.HeightOffset;
            ImGui.InputDouble($"Surface offset", ref temp);
            if(ImGui.Button("Change Offset"))
            {
                LandmarkMeshRenderer.HeightOffset = temp;
            }
        }
    }
}
