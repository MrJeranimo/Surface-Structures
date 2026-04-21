using Brutal.Logging;
using Brutal.Numerics;
using Brutal.VulkanApi;
using HarmonyLib;
using KSA;

namespace Surface_Structures
{
    [HarmonyPatch]
    public class PlanetRendererPatch
    {
        [HarmonyPatch(typeof(PartModelRenderer), "UpdateRenderData")]
        [HarmonyPrefix]
        public static void SubmitLandmarkInstances(Viewport viewport, int frameIndex)
        {
            foreach (var renderer in LandmarkRenderableRegistry.All)
                renderer.SubmitInstance(viewport, frameIndex);
        }

        [HarmonyPatch(typeof(LocationReference), "DrawUi")]
        [HarmonyPrefix]
        public static bool HideLocationsPatch(Viewport inViewport, Celestial celestial, LocationReference __instance)
        {
            if(!SurfaceStructureParser.LocationStructs.TryGetValue(__instance.Id, out LocationStruct? location))
                return true;  // If there is no specified LocationStruct, DrawUi

            if (location.Celestial == celestial.Id && !location.Visible)
                return false;  // If the celestial matches and the location has Visbile = false, don't DrawUi

            return true;  // Else, DrawUi
        }
    }
}
