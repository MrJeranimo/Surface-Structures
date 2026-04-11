using Brutal.Numerics;
using System.Globalization;
using System.Xml.Linq;

namespace Surface_Structures
{
    public class SurfaceStructureParser
    {
        private static List<LandmarkStructure> _landmarkStructures = new List<LandmarkStructure>();

        public static void ParseFile(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (var element in doc.Descendants("SurfaceStructure"))
            {
                string? id = element.Attribute("id")?.Value;
                string? landmarkName = element.Element("Landmark")?.Attribute("name")?.Value;
                string? meshID = element.Element("MeshID")?.Attribute("id")?.Value;

                if (string.IsNullOrEmpty(meshID) || string.IsNullOrEmpty(landmarkName) || string.IsNullOrEmpty(id))
                    continue;

                float3 position = ParseFloat3(element.Element("Position"));
                float3 rotation = ParseFloat3(element.Element("Rotation"));
                float3 scale = ParseFloat3(element.Element("Scale"));
                    
                if (scale == new float3(0, 0, 0))
                    scale = new float3(1, 1, 1);

                _landmarkStructures.Add(new LandmarkStructure(id, landmarkName, meshID, position, rotation, scale));
            }
        }

        private static float3 ParseFloat3(XElement? element)
        {
            if (element == null)
                return new float3(0, 0, 0);

            float x = ParseFloat(element.Attribute("x"));
            float y = ParseFloat(element.Attribute("y"));
            float z = ParseFloat(element.Attribute("z"));

            return new float3(x, y, z);
        }

        private static float ParseFloat(XAttribute? attribute)
        {
            if (attribute == null)
                return 0f;

            return float.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
                ? result
                : 0f;
        }

        public static void RegisterLandmarkStructures()
        {
            foreach (var structure in _landmarkStructures)
            {
                if (!LandmarkRenderableRegistry.MeshMap.ContainsKey(structure.LandmarkName))
                {
                    LandmarkRenderableRegistry.MeshMap[structure.LandmarkName] = new LandmarkStructure[] { structure };
                }
                else
                {
                    var existing = LandmarkRenderableRegistry.MeshMap[structure.LandmarkName].ToList();
                    existing.Add(structure);
                    LandmarkRenderableRegistry.MeshMap[structure.LandmarkName] = existing.ToArray();
                }
            }
        }
    }
}
