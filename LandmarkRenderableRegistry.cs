using Brutal.Numerics;

namespace Surface_Structures {
    public static class LandmarkRenderableRegistry
    {
        private static readonly List<LandmarkMeshRenderer> _renderers = new();
        public static Dictionary<string, LandmarkStructure[]> MeshMap = new();

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
