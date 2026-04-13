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
        private static int _selectedLandmarkIndex = 0;
        private static string _landmarkNames = string.Empty;
        private static LandmarkReference? _selectedLandmark = null;
        private static Celestial? _selectedLandmarkCelestial = null;

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

                    if(ImGui.BeginTabItem("Landmark Editor"))
                    {
                        DrawLandmarkEditorTab();
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
                ImGui.Combo("Landmark", ref _selectedLandmarkIndex, _landmarkNames, _landmarkNames.Length);
                ImGui.Spacing();
                if (ImGui.Button("Change Landmark"))
                {
                    findLandmark();
                    ChangeStructureLandmark();
                }
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.InputFloat3("Position", ref _selectedStructure.Position);
                ImGui.Spacing();
                ImGui.InputFloat3("Rotation", ref _selectedStructure.Rotation);
                ImGui.Spacing();
                ImGui.InputFloat3("Scale", ref _selectedStructure.Scale);
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

        private static void DrawLandmarkEditorTab()
        {
            ImGui.TextWrapped("Saving changes is not supported yet");
            ImGui.Combo("Landmark", ref _selectedLandmarkIndex, _landmarkNames, _landmarkNames.Length);
            ImGui.Spacing();
            if (ImGui.Button("Select"))
            {
                findLandmark();
            }
            if (_selectedLandmark != null)
            {
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.InputDouble("Latitude (Degrees)", ref _selectedLandmark.Latitude.Degrees);
                ImGui.Spacing();
                ImGui.InputDouble("Longitude (Degrees)", ref _selectedLandmark.Longitude.Degrees);
                ImGui.Spacing();
                if (ImGui.Button("Update Landmark"))
                {
                    _selectedLandmark.Latitude = RadianReference.FromDegrees(_selectedLandmark.Latitude.Degrees);
                    _selectedLandmark.Longitude = RadianReference.FromDegrees(_selectedLandmark.Longitude.Degrees);
                    _selectedLandmark.OnDataLoad(Mod.Empty);
                }
            }
        }

        public static void CreateStructureNames()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (landmark, structures) in LandmarkRenderableRegistry.MeshMap)
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
            foreach (var (landmark, structures) in LandmarkRenderableRegistry.MeshMap)
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

        public static void CreateLandmarkNames()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                foreach (LandmarkReference landmark in celestial.BodyTemplate.Locations.OfType<LandmarkReference>())
                {
                    sb.Append(landmark.Id).Append('\0');
                }
            }
            _landmarkNames = sb.ToString();
        }

        private static void findLandmark()
        {
            int index = 0;
            foreach (Celestial celestial in Universe.CurrentSystem!.All.OfType<Celestial>())
            {
                foreach (LandmarkReference landmark in celestial.BodyTemplate.Locations.OfType<LandmarkReference>())
                {
                    if (index == _selectedLandmarkIndex)
                    {
                        _selectedLandmark = landmark;
                        _selectedLandmarkCelestial = celestial;
                        return;
                    }
                    index++;
                }
            }
        }

        private static void ChangeStructureLandmark()
        {
            LandmarkMeshRenderer renderer = LandmarkRenderableRegistry.All[_selectedStructure!.RendererIndex];
            renderer.UpdateLandmark(_selectedLandmark!, _selectedLandmarkCelestial!);
            _selectedStructure!.LandmarkName = _selectedLandmark!.Id;
        }
    }
}
