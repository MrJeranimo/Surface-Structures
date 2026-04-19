using Brutal.Numerics;

namespace Surface_Structures
{
    public class LandmarkStructure
    {
        public readonly string ID;
        public string LandmarkName;
        public readonly string PartID;
        public readonly string FilePath;
        public float3 Position = new float3(0, 0, 0);
        public float3 Rotation = new float3(0, 0, 0);
        public float3 Scale = new float3(1, 1, 1);
        public bool Visible = true;
        public bool Loaded = false;
        public int RendererIndex = -1;

        public LandmarkStructure(string id, string landmarkName, string partID, string filePath)
        {
            ID = id;
            LandmarkName = landmarkName;
            PartID = partID;
            FilePath = filePath;
        }

        public LandmarkStructure(string id, string landmarkName, string partID, string filePath, float3 position)
        {
            ID = id;
            LandmarkName = landmarkName;
            PartID = partID;
            FilePath = filePath;
            Position = position;
        }

        public LandmarkStructure(string id, string landmarkName, string partID, string filePath, float3 position, float3 rotation, float3 scale, bool visible)
        {
            ID = id;
            LandmarkName = landmarkName;
            PartID = partID;
            FilePath = filePath;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Visible = visible;
        }

        public void SetLoaded() { Loaded = true; }
    }
}
