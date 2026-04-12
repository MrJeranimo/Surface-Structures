using Brutal.ImGuiApi;
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

            if(ImGui.Begin("Structure Editor"))
            {
                ImGui.Combo("Structure", ref _selectedStructureIndex, _structureNames, _structureNames.Length);
                if(ImGui.Button("Select"))
                {
                    findStructure();
                }
                if (_selectedStructure != null)
                {
                    ImGui.Separator();
                    ImGui.InputFloat3("Position", ref _selectedStructure.Position);
                    ImGui.Spacing();
                    ImGui.InputFloat3("Rotation", ref _selectedStructure.Rotation);
                    ImGui.Spacing();
                    ImGui.InputFloat3("Scale", ref _selectedStructure.Scale);
                    ImGui.Spacing();
                    ImGui.Checkbox("Visible", ref _selectedStructure.Visible);
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
