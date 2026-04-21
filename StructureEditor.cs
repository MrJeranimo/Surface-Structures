using Brutal.ImGuiApi;
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
        private static int _selectedLocationIndex = 0;
        private static string _locationNames = string.Empty;
        private static LocationReference? _selectedLocation = null;
        private static Celestial? _selectedLocationCelestial = null;

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
                ImGui.Combo("Location", ref _selectedLocationIndex, _locationNames, _locationNames.Length);
                ImGui.Spacing();
                if (ImGui.Button("Change Location"))
                {
                    findLocation();
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
            ImGui.TextWrapped("Saving changes is not supported yet");
            ImGui.Combo("Location", ref _selectedLocationIndex, _locationNames, _locationNames.Length);
            ImGui.Spacing();
            if (ImGui.Button("Select"))
            {
                findLocation();
            }
            if (_selectedLocation != null)
            {
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.InputDouble("Latitude (Degrees)", ref _selectedLocation.Latitude.Degrees, 0.1, 0.5);
                ImGui.Spacing();
                ImGui.InputDouble("Longitude (Degrees)", ref _selectedLocation.Longitude.Degrees, 0.1, 0.5);
                ImGui.Spacing();
                if (ImGui.Button("Update Location"))
                {
                    _selectedLocation.Latitude = RadianReference.FromDegrees(_selectedLocation.Latitude.Degrees);
                    _selectedLocation.Longitude = RadianReference.FromDegrees(_selectedLocation.Longitude.Degrees);
                }
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
                    sb.Append(structure.ID).Append('\0');
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

        public static void CreateLocationNames()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                foreach (LocationReference location in celestial.BodyTemplate.Locations)
                {
                    sb.Append(location.Id).Append('\0');
                }
            }
            _locationNames = sb.ToString();
        }

        private static void findLocation()
        {
            int index = 0;
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                foreach (LocationReference location in celestial.BodyTemplate.Locations)
                {
                    if (index == _selectedLocationIndex)
                    {
                        _selectedLocation = location;
                        _selectedLocationCelestial = celestial;
                        return;
                    }
                    index++;
                }
            }
        }

        private static void ChangeStructureLocation()
        {
            LandmarkMeshRenderer renderer = LandmarkRenderableRegistry.All[_selectedStructure!.RendererIndex];
            renderer.UpdateLandmark(_selectedLocation!, _selectedLocationCelestial!);
            _selectedStructure!.LocationName = _selectedLocation!.Id;
        }
    }
}
