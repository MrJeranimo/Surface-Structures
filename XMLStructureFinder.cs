using Brutal.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surface_Structures
{
    public class XMLStructureFinder
    {
        private static string _modsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My Games", "Kitten Space Agency", "mods"
            );

        public static void FindModFolder()
        {
            string modsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My Games", "Kitten Space Agency", "mods"
            );

            if (!Directory.Exists(modsPath))
            {
                DefaultCategory.Log.Error("Surface Structures - Mods folder not found: " + modsPath);
            }

            _modsPath = modsPath;
        }

        public static Dictionary<string, string[]> FindSurfaceStructuresFiles()
        {
            Dictionary<string, string[]> results = new Dictionary<string, string[]>();

            // Get each mod folder (top-level only)
            string[] modFolders = Directory.GetDirectories(_modsPath, "*", SearchOption.TopDirectoryOnly);

            foreach (string modFolder in modFolders)
            {
                // Search recursively within each mod folder for the file
                string[] matches = Directory.EnumerateFiles(modFolder, "Surface Structures.xml", SearchOption.AllDirectories).ToArray();

                if (matches != null)
                {
                    string modName = Path.GetFileName(modFolder);
                    results[modName] = matches;
                }
            }

            return results;
        }
    }
}
