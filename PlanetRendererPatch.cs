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
        [HarmonyPatch(typeof(PlanetRenderer), "GenerateMeshData")]
        [HarmonyPostfix]
        public static void DrawLandmarkMeshes(CommandBuffer commandBuffer, Viewport inViewport, int frameIndex)
        {
            foreach (var renderer in LandmarkRenderableRegistry.All)
                renderer.Draw(inViewport);
        }
    }
}
