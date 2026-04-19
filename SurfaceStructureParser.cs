using Brutal.Numerics;
using KSA;
using System.Globalization;
using System.Xml.Linq;

namespace Surface_Structures
{
    public class SurfaceStructureParser
    {
        private static List<KeyValuePair<string, LandmarkReference>> _landmarks = new List<KeyValuePair<string, LandmarkReference>>();
        private static List<LandmarkStructure> _landmarkStructures = new List<LandmarkStructure>();

        public static List<KeyValuePair<string, LandmarkReference>> Landmarks => _landmarks;

        public static void ParseFile(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Descendants("Landmark"))
            {
                string? name = element.Attribute("Name")?.Value;
                string? celestial = element.Element("Celestial")?.Attribute("Id")?.Value;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(celestial))
                    continue;

                double longitude = ParseDouble(element.Element("Longitude")?.Attribute("Degrees")?.Value);
                double latitude = ParseDouble(element.Element("Latitude")?.Attribute("Degrees")?.Value);
                bool visible = ParseBool(element.Element("Visible")?.Attribute("Value")?.Value);

                LandmarkReference landmark = new LandmarkReference();
                landmark.Latitude = RadianReference.FromDegrees(latitude);
                landmark.Longitude = RadianReference.FromDegrees(longitude);
                landmark.Id = name;
                landmark.OnDataLoad(Mod.Empty);

                _landmarks.Add(new KeyValuePair<string, LandmarkReference>(celestial, landmark));
            }

            foreach (XElement element in doc.Descendants("SurfaceStructure"))
            {
                string? id = element.Attribute("Id")?.Value;
                string? landmarkName = element.Element("Landmark")?.Attribute("Name")?.Value;
                string? partID = element.Element("PartId")?.Attribute("Id")?.Value;

                if (string.IsNullOrEmpty(partID) || string.IsNullOrEmpty(landmarkName) || string.IsNullOrEmpty(id))
                    continue;

                float3 position = ParseFloat3(element.Element("Position"));
                float3 rotation = ParseFloat3(element.Element("Rotation"));
                float3 scale = ParseFloat3(element.Element("Scale"));
                    
                if (scale == new float3(0, 0, 0))
                    scale = new float3(1, 1, 1);

                bool visible = ParseBool(element.Element("Visible")?.Attribute("value")?.Value);

                _landmarkStructures.Add(new LandmarkStructure(id, landmarkName, partID, filePath, position, rotation, scale, visible));
            }
        }

        private static float3 ParseFloat3(XElement? element)
        {
            if (element == null)
                return new float3(0, 0, 0);

            float x = ParseFloat(element.Attribute("X"));
            float y = ParseFloat(element.Attribute("Y"));
            float z = ParseFloat(element.Attribute("Z"));

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

        private static double ParseDouble(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return 0.0;
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result)
                ? result
                : 0.0;
        }

        private static bool ParseBool(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return true;
            return bool.TryParse(value, out bool result) ? result : true;
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
                string? id = element.Attribute("Id")?.Value;
                if (string.IsNullOrEmpty(id))
                    continue;

                if (structure.ID != id)
                    continue;

                XElement? landmark = element.Element("Landmark");
                string? landmarkName = landmark?.Attribute("Name")?.Value;
                if(string.IsNullOrEmpty(landmarkName))
                    continue;

                if(landmark != null)
                    landmark.SetAttributeValue("Name", structure.LandmarkName);

                WriteFloat3(element.Element("Position"), structure.Position);
                WriteFloat3(element.Element("Rotation"), structure.Rotation);
                WriteFloat3(element.Element("Scale"), structure.Scale);

                XElement? visibleElement = element.Element("Visible");
                if (visibleElement != null)
                    visibleElement.SetAttributeValue("Value", structure.Visible.ToString().ToLower());
            }

            doc.Save(structure.FilePath);
        }

        private static void WriteFloat3(XElement? element, float3 value)
        {
            if (element == null)
                return;

            element.SetAttributeValue("X", value.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttributeValue("Y", value.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttributeValue("Z", value.Z.ToString(CultureInfo.InvariantCulture));
        }
    }
}
