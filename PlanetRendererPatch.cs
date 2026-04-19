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
    }
}
