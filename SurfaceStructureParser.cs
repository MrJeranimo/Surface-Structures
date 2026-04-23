using Brutal.Logging;
using Brutal.Numerics;
using KSA;
using System.Globalization;
using System.Xml.Linq;

namespace Surface_Structures
{
    public class SurfaceStructureParser
    {
        private static List<KeyValuePair<string, LocationReference>> _locations = new List<KeyValuePair<string, LocationReference>>();
        private static List<LandmarkStructure> _landmarkStructures = new List<LandmarkStructure>();
        public static Dictionary<string, LocationData> LocationStructs = new Dictionary<string, LocationData>();

        public static List<KeyValuePair<string, LocationReference>> Locations => _locations;

        public static void ParseFile(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            foreach (XElement element in doc.Descendants("Mountain"))
            {
                string? name = element.Attribute("Name")?.Value;
                string? celestial = element.Element("Celestial")?.Attribute("Id")?.Value;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(celestial))
                    continue;

                double latitude = ParseDouble(element.Element("Latitude")?.Attribute("Degrees")?.Value);
                double longitude = ParseDouble(element.Element("Longitude")?.Attribute("Degrees")?.Value);
                bool visible = ParseBool(element.Element("Visible")?.Attribute("Value")?.Value);

                MountainReference mountain = new MountainReference();
                mountain.Latitude = RadianReference.FromDegrees(latitude);
                mountain.Longitude = RadianReference.FromDegrees(longitude);
                mountain.Id = name;
                mountain.OnDataLoad(Mod.Empty);

                _locations.Add(new KeyValuePair<string, LocationReference>(celestial, mountain));
                LocationStructs.Add(name, new LocationData(name, celestial, visible, filePath, latitude, longitude, mountain));
            }

            foreach (XElement element in doc.Descendants("City"))
            {
                string? name = element.Attribute("Name")?.Value;
                string? celestial = element.Element("Celestial")?.Attribute("Id")?.Value;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(celestial))
                    continue;

                double latitude = ParseDouble(element.Element("Latitude")?.Attribute("Degrees")?.Value);
                double longitude = ParseDouble(element.Element("Longitude")?.Attribute("Degrees")?.Value);
                bool visible = ParseBool(element.Element("Visible")?.Attribute("Value")?.Value);

                CityReference city = new CityReference();
                city.Latitude = RadianReference.FromDegrees(latitude);
                city.Longitude = RadianReference.FromDegrees(longitude);
                city.Id = name;
                city.OnDataLoad(Mod.Empty);

                _locations.Add(new KeyValuePair<string, LocationReference>(celestial, city));
                LocationStructs.Add(name, new LocationData(name, celestial, visible, filePath, latitude, longitude, city));
            }

            foreach (XElement element in doc.Descendants("Landmark"))
            {
                string? name = element.Attribute("Name")?.Value;
                string? celestial = element.Element("Celestial")?.Attribute("Id")?.Value;

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(celestial))
                    continue;

                double latitude = ParseDouble(element.Element("Latitude")?.Attribute("Degrees")?.Value);
                double longitude = ParseDouble(element.Element("Longitude")?.Attribute("Degrees")?.Value);
                bool visible = ParseBool(element.Element("Visible")?.Attribute("Value")?.Value);

                LandmarkReference landmark = new LandmarkReference();
                landmark.Latitude = RadianReference.FromDegrees(latitude);
                landmark.Longitude = RadianReference.FromDegrees(longitude);
                landmark.Id = name;
                landmark.OnDataLoad(Mod.Empty);

                _locations.Add(new KeyValuePair<string, LocationReference>(celestial, landmark));
                LocationStructs.Add(name, new LocationData(name, celestial, visible, filePath, latitude, longitude, landmark));
            }

            foreach (XElement element in doc.Descendants("SurfaceStructure"))
            {
                string? id = element.Attribute("Id")?.Value;
                string? locationName = element.Element("Location")?.Attribute("Name")?.Value;
                string? partID = element.Element("PartId")?.Attribute("Id")?.Value;

                if (string.IsNullOrEmpty(partID) || string.IsNullOrEmpty(locationName) || string.IsNullOrEmpty(id))
                    continue;

                float3 position = ParseFloat3(element.Element("Position"));
                float3 rotation = ParseFloat3(element.Element("Rotation"));
                float3 scale = ParseFloat3(element.Element("Scale"));
                    
                if (scale == new float3(0, 0, 0))
                    scale = new float3(1, 1, 1);

                bool visible = ParseBool(element.Element("Visible")?.Attribute("value")?.Value);

                _landmarkStructures.Add(new LandmarkStructure(id, locationName, partID, filePath, position, rotation, scale, visible));
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
                if (!LandmarkRenderableRegistry.MeshMap.ContainsKey(structure.LocationName))
                {
                    LandmarkRenderableRegistry.MeshMap[structure.LocationName] = new LandmarkStructure[] { structure };
                }
                else
                {
                    var existing = LandmarkRenderableRegistry.MeshMap[structure.LocationName].ToList();
                    existing.Add(structure);
                    LandmarkRenderableRegistry.MeshMap[structure.LocationName] = existing.ToArray();
                }
            }
        }

        public static void SaveStructure(LandmarkStructure structure)
        {
            XDocument doc = XDocument.Load(structure.FilePath);

            foreach (var element in doc.Descendants("SurfaceStructure"))
            {
                string? id = element.Attribute("Id")?.Value;
                if (string.IsNullOrEmpty(id) || structure.Id != id)
                    continue;

                XElement? location = element.Element("Location");
                if(location != null)
                    location.SetAttributeValue("Name", structure.LocationName);

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

        public static void SaveLocation(LocationData location)
        {
            XDocument doc = XDocument.Load(location.Filepath);

            var mountains = doc.Descendants("Mountain");
            var cities = doc.Descendants("City");
            var landmarks = doc.Descendants("Landmark");

            var locations = mountains.Concat(cities.Concat(landmarks));

            foreach (XElement element in locations)
            {
                string? name = element.Attribute("Name")?.Value;
                if (string.IsNullOrEmpty(name) || name != location.Name)
                    continue;

                XElement? celestial = element.Element("Celestial");
                if (celestial != null)
                    celestial.SetAttributeValue("Id", location.Celestial);

                XElement? latitude = element.Element("Latitude");
                if (latitude != null)
                    latitude.SetAttributeValue("Degrees", location.Latitude.ToString());

                XElement? longitude = element.Element("Longitude");
                if (longitude != null)
                    longitude.SetAttributeValue("Degrees", location.Longitude.ToString());

                XElement? visibleElement = element.Element("Visible");
                if (visibleElement != null)
                    visibleElement.SetAttributeValue("Value", location.Visible.ToString().ToLower());
            }

            doc.Save(location.Filepath);
        }
    }
}
