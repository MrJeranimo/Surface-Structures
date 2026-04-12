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

                string? visibleString = element.Element("Visible")?.Attribute("value")?.Value;
                bool visible = true;

                if (!string.IsNullOrEmpty(visibleString))
                    visible = bool.TryParse(visibleString, out bool result) ? result : true;

                _landmarkStructures.Add(new LandmarkStructure(id, landmarkName, meshID, filePath, position, rotation, scale, visible));
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

        public static void SaveStructure(LandmarkStructure structure)
        {
            XDocument doc = XDocument.Load(structure.FilePath);

            foreach (var element in doc.Descendants("SurfaceStructure"))
            {
                string? id = element.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(id))
                    continue;

                if (structure.ID != id)
                    continue;

                WriteFloat3(element.Element("Position"), structure.Position);
                WriteFloat3(element.Element("Rotation"), structure.Rotation);
                WriteFloat3(element.Element("Scale"), structure.Scale);

                var visibleElement = element.Element("Visible");
                if (visibleElement != null)
                    visibleElement.SetAttributeValue("value", structure.Visible.ToString().ToLower());
            }

            doc.Save(structure.FilePath);
        }

        private static void WriteFloat3(XElement? element, float3 value)
        {
            if (element == null)
                return;

            element.SetAttributeValue("x", value.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttributeValue("y", value.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttributeValue("z", value.Z.ToString(CultureInfo.InvariantCulture));
        }
    }
}
