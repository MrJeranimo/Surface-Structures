using Brutal.ImGuiApi;
using Brutal.Logging;
using Brutal.Numerics;
using KSA;
using System.Text;

namespace Surface_Structures
{
    public class StructureEditor
    {
        public static bool ShowEditorWindow = false;
        private static LandmarkStructure? _selectedStructure = null;
        private static int _selectedStructureIndex = 0;
        private static string _structureNames = string.Empty;
        private static int _selectedStructureLocationIndex = 0;
        private static string _structureLocationNames = string.Empty;
        private static LocationReference? _selectedStructureLocation = null;
        private static int _selectedLocationIndex = 0;
        private static string _locationNames = string.Empty;
        private static LocationReference? _selectedLocation = null;
        private static Celestial? _selectedLocationCelestial = null;
        private static LocationData? _selectedLocationData = null;
        private static string _celestialNames = string.Empty;
        private static int _selectedCelestialIndex = 0;
        private static Celestial? _selectedCelestial = null;

        public static void DrawWindow() 
        {
            if (!ShowEditorWindow)
                return;

            ImGui.SetNextWindowSize(new float2(400, 500), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Structure Editor", ref ShowEditorWindow))
            {
                if (ImGui.BeginTabBar("StructureEditorTabs"))
                {
                    if(ImGui.BeginTabItem("Structure Editor"))
                    {
                        DrawStructureEditorTab();
                        ImGui.EndTabItem();
                    }

                    if(ImGui.BeginTabItem("Location Editor"))
                    {
                        DrawLocationEditorTab();
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }

        private static void DrawStructureEditorTab()
        {
            ImGui.TextWrapped("Changes can be saved back to the XML file for persistence.");
            ImGui.Spacing();
            ImGui.Combo("Structure", ref _selectedStructureIndex, _structureNames, _structureNames.Length);
            ImGui.Spacing();
            if (ImGui.Button("Select"))
            {
                findStructure();
            }
            if (_selectedStructure != null)
            {
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.Combo("Location", ref _selectedStructureLocationIndex, _structureLocationNames, 10);
                ImGui.Spacing();
                if (ImGui.Button("Change Location"))
                {
                    findStructureLocation();
                    ChangeStructureLocation();
                }
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.DragFloat3("Position", ref _selectedStructure.Position);
                ImGui.Spacing();
                ImGui.DragFloat3("Rotation", ref _selectedStructure.Rotation, vMin: -360f, vMax: 360f);
                ImGui.Spacing();
                ImGui.DragFloat3("Scale", ref _selectedStructure.Scale);
                ImGui.Spacing();
                ImGui.Checkbox("Visible", ref _selectedStructure.Visible);
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                if (ImGui.CollapsingHeader("Advanced"))
                {
                    ImGui.Spacing();
                    if (ImGui.Button("Reset Position"))
                    {
                        _selectedStructure.Position = new float3(0, 0, 0);
                    }
                    ImGui.Spacing();
                    if (ImGui.Button("Reset Rotation"))
                    {
                        _selectedStructure.Rotation = new float3(0, 0, 0);
                    }
                    ImGui.Spacing();
                    if (ImGui.Button("Reset Scale"))
                    {
                        _selectedStructure.Scale = new float3(1, 1, 1);
                    }
                    ImGui.Spacing();
                    if (ImGui.Button("Reset All"))
                    {
                        _selectedStructure.Position = new float3(0, 0, 0);
                        _selectedStructure.Rotation = new float3(0, 0, 0);
                        _selectedStructure.Scale = new float3(1, 1, 1);
                        _selectedStructure.Visible = true;
                    }
                }
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                if (ImGui.Button("Save Changes"))
                {
                    SurfaceStructureParser.SaveStructure(_selectedStructure);
                }
            }
        }

        private static void DrawLocationEditorTab()
        {
            ImGui.TextWrapped("Only Modded Locations can be edited. Changes can be saved back to the XML files for persistence.");
            ImGui.Spacing();
            ImGui.Combo("Location", ref _selectedLocationIndex, _locationNames, 10);
            ImGui.Spacing();
            if (ImGui.Button("Select"))
            {
                findLocation();
            }
            if (_selectedLocationData != null && _selectedLocation != null)
            {
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.Combo("Celestial On", ref _selectedCelestialIndex, _celestialNames, 10);
                ImGui.Spacing();
                if (ImGui.Button("Update Celestial On"))
                {
                    findCelestial();
                    updateLocation();
                }
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.InputDouble("Latitude (Degrees)", ref _selectedLocationData.Latitude, 0.01, 0.1);
                ImGui.Spacing();
                ImGui.InputDouble("Longitude (Degrees)", ref _selectedLocationData.Longitude, 0.01, 0.1);
                ImGui.Spacing();
                ImGui.Checkbox("Visible", ref _selectedLocationData.Visible);
                ImGui.Spacing();
                if (ImGui.Button("Update Location"))
                {
                    _selectedLocation.Latitude = RadianReference.FromDegrees(_selectedLocationData.Latitude);
                    _selectedLocation.Longitude = RadianReference.FromDegrees(_selectedLocationData.Longitude);
                    _selectedLocation.OnDataLoad(Mod.Empty);
                }
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                if (ImGui.Button("Save Changes"))
                {
                    SurfaceStructureParser.SaveLocation(_selectedLocationData);
                }
            }
        }

        public static void CreateCelestialNames()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                sb.Append(celestial.Id).Append('\0');
            }
            _celestialNames = sb.ToString();
        }

        private static void findCelestial()
        {
            int index = 0;
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                if (index == _selectedCelestialIndex)
                {
                    _selectedCelestial = celestial;
                    return;
                }
                index++;
            }
        }

        private static void updateLocation()
        {
            if (_selectedLocationData != null && _selectedCelestial != null && _selectedLocation != null)
            {
                string oldCelestialName = _selectedLocationData.Celestial;
                Celestial? oldCelstial = findCelestial(oldCelestialName);
                _selectedLocationData.Celestial = _selectedCelestial.Id;
                if (oldCelstial != null)
                {
                    oldCelstial.BodyTemplate.Locations.Remove(_selectedLocation);
                    _selectedCelestial.BodyTemplate.Locations.Add(_selectedLocation);
                    UpdateStructuresCelestial();
                }
            }
            else
            {
                DefaultCategory.Log.Error("Error updating Celestial On");
            }
        }

        public static Celestial? findCelestial(string celestialName)
        {
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                if (celestial.Id == celestialName)
                    return celestial;
            }
            return null;
        }

        private static void UpdateStructuresCelestial()
        {
            foreach (var structure in LandmarkRenderableRegistry.All)
            {
                if (structure.GetLocation() == _selectedLocation)
                    structure.SetCelestial(_selectedCelestial!);
            }
        }

        public static void CreateStructureNames()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (location, structures) in LandmarkRenderableRegistry.MeshMap)
            {
                foreach (var structure in structures)
                {
                    if (!structure.Loaded)
                        continue;
                    sb.Append(structure.Id).Append('\0');
                }
            }
            _structureNames = sb.ToString();
        }

        private static void findStructure()
        {
            int index = 0;
            foreach (var (location, structures) in LandmarkRenderableRegistry.MeshMap)
            {
                foreach (var structure in structures)
                {
                    if (!structure.Loaded)
                        continue;
                    if (index == _selectedStructureIndex)
                    {
                        _selectedStructure = structure;
                        return;
                    }
                    index++;
                }
            }
        }

        public static void CreateStructureLocationNames()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                foreach (LocationReference location in celestial.BodyTemplate.Locations)
                {
                    sb.Append(location.Id).Append('\0');
                }
            }
            _structureLocationNames = sb.ToString();
        }

        public static void findStructureLocation()
        {
            int index = 0;
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                foreach (LocationReference location in celestial.BodyTemplate.Locations)
                {
                    if (index == _selectedStructureLocationIndex)
                    {
                        _selectedStructureLocation = location;
                        _selectedLocationCelestial = celestial;
                        return;
                    }
                    index++;
                }
            }
        }

        public static void CreateLocationNames()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (locationName, location) in SurfaceStructureParser.LocationStructs)
            {
                sb.Append(location.Name).Append('\0');
            }
            _locationNames = sb.ToString();
        }

        private static void findLocation()
        {
            int index = 0;
            foreach (var (locationName, location) in SurfaceStructureParser.LocationStructs)
            {
                if (index == _selectedLocationIndex)
                {
                    _selectedLocation = location.Location;
                    _selectedLocationData = location;
                    Celestial? Celestial = null;
                    foreach(Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
                    {
                        if (celestial.Id == location.Celestial)
                            Celestial = celestial;
                    }
                    if (Celestial != null)
                    {
                        _selectedLocationCelestial = Celestial;
                        setCelestialIndex(Celestial);
                    }
                    return;
                }
                index++;
            }
        }

        private static void setCelestialIndex(Celestial currentCelestal)
        {
            int index = 0;
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                if (currentCelestal == celestial)
                {
                    _selectedCelestialIndex = index;
                    return;
                }
                index++;
            }
        }

        private static void ChangeStructureLocation()
        {
            LandmarkMeshRenderer renderer = LandmarkRenderableRegistry.All[_selectedStructure!.RendererIndex];
            renderer.UpdateLandmark(_selectedStructureLocation!, _selectedLocationCelestial!);
            _selectedStructure!.LocationName = _selectedStructureLocation!.Id;
        }
    }
}
