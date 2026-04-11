using Brutal.Numerics;

namespace Surface_Structures
{
    public struct LandmarkStructure
    {
        public string ID;
        public string LandmarkName;
        public string MeshID;
        public float3 Position = new float3(0, 0, 0);
        public float3 Rotation = new float3(0, 0, 0);
        public float3 Scale = new float3(1, 1, 1);

        public LandmarkStructure(string id, string landmarkName, string meshID)
        {
            ID = id;
            LandmarkName = landmarkName;
            MeshID = meshID;
        }

        public LandmarkStructure(string id, string landmarkName, string meshID, float3 position)
        {
            ID = id;
            LandmarkName = landmarkName;
            MeshID = meshID;
            Position = position;
        }

        public LandmarkStructure(string id, string landmarkName, string meshID, float3 position, float3 rotation, float3 scale)
        {
            ID = id;
            LandmarkName = landmarkName;
            MeshID = meshID;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
