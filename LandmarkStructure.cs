using Brutal.Numerics;

namespace Surface_Structures
{
    public class LandmarkStructure
    {
        public readonly string Id;
        public string LocationName;
        public readonly string PartID;
        public readonly string FilePath;
        public float3 Position = new float3(0, 0, 0);
        public float3 Rotation = new float3(0, 0, 0);
        public float3 Scale = new float3(1, 1, 1);
        public bool Visible = true;
        public bool Loaded = false;
        public int RendererIndex = -1;

        public LandmarkStructure(string id, string locationName, string partID, string filePath)
        {
            Id = id;
            LocationName = locationName;
            PartID = partID;
            FilePath = filePath;
        }

        public LandmarkStructure(string id, string locationName, string partID, string filePath, float3 position)
        {
            Id = id;
            LocationName = locationName;
            PartID = partID;
            FilePath = filePath;
            Position = position;
        }

        public LandmarkStructure(string id, string locationName, string partID, string filePath, float3 position, float3 rotation, float3 scale, bool visible)
        {
            Id = id;
            LocationName = locationName;
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
