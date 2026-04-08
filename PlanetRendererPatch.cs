using Brutal.Logging;
using HarmonyLib;
using KSA;
using Brutal.VulkanApi;

namespace Surface_Structures
{
    [HarmonyPatch]
    public class PlanetRendererPatch
    {
        [HarmonyPatch(typeof(PlanetRenderer), "UpdateGroundClutter")]
        [HarmonyPostfix]
        public static void DrawLandmarkMeshes(CommandBuffer commandBuffer, Viewport viewport, int frameIndex)
        {
            foreach (var renderer in LandmarkRenderableRegistry.All)
                renderer.Draw(viewport);
        }
    }
}
