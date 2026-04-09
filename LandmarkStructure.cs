using Brutal.Numerics;

namespace Surface_Structures
{
    public struct LandmarkStructure
    {
        public string MeshID;
        public float3 Position = new float3(0, 0, 0);
        public float3 Rotation = new float3(0, 0, 0);
        public float3 Scale = new float3(1, 1, 1);

        public LandmarkStructure(string meshID)
        {
            MeshID = meshID;
        }

        public LandmarkStructure(string meshID, float3 position)
        {
            MeshID = meshID;
            Position = position;
        }

        public LandmarkStructure(string meshID, float3 position, float3 rotation, float3 scale)
        {
            MeshID = meshID;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
