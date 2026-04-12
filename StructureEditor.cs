using Brutal.ImGuiApi;
using Brutal.Numerics;
using System.Text;

namespace Surface_Structures
{
    public class StructureEditor
    {
        public static bool ShowEditorWindow = false;
        private static LandmarkStructure? _selectedStructure = null;
        private static int _selectedStructureIndex = 0;
        private static string _structureNames = string.Empty;

        public static void DrawWindow() 
        {
            if (!ShowEditorWindow)
                return;

            ImGui.SetNextWindowSize(new float2(400, 500), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Structure Editor", ref ShowEditorWindow))
            {
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
            ImGui.End();
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
    }
}
