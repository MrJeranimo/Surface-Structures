using Brutal.Numerics;

namespace Surface_Structures {
    public static class LandmarkRenderableRegistry
    {
        private static readonly List<LandmarkMeshRenderer> _renderers = new();
        public static Dictionary<string, LandmarkStructure[]> MeshMap = new()
        {
            { "CCSFS LC-39A", [new LandmarkStructure("MetalRoughSpheres", new float3(100, 0, 100)), new LandmarkStructure("KittenHelmetGlb", new float3(-100, -100, -100), new float3(0, 0, 0), new float3(1, 1, 1))] },
        };

        public static IReadOnlyList<LandmarkMeshRenderer> All => _renderers;

        public static void Add(LandmarkMeshRenderer renderer)
        {
            _renderers.Add(renderer);
        }

        public static void Remove(LandmarkMeshRenderer renderer)
        {
            _renderers.Remove(renderer);
            renderer.Dispose();
        }

        public static void Clear()
        {
            foreach (var renderer in _renderers)
                renderer.Dispose();
            _renderers.Clear();
        }
    }
}
