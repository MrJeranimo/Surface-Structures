using KSA;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Surface_Structures
{
    public class ConfigReader
    {
        private readonly JsonNode _root;
        private readonly string _filePath;

        public ConfigReader(string filePath = "Config.json")
        {
            _filePath = Path.Combine(ModLibrary.LocalModsFolderPath, "Surface Structures", filePath);

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Config file not found: {_filePath}");

            string json = File.ReadAllText(_filePath);
            _root = JsonNode.Parse(json)
                ?? throw new InvalidDataException("Config file is empty or invalid JSON.");
        }

        /// <summary>
        /// Gets a value by dot-notated key path (e.g. "Database.Host").
        /// Returns null if the key doesn't exist.
        /// </summary>
        public T? Get<T>(string keyPath)
        {
            JsonNode? node = Traverse(keyPath);
            if (node is null) return default;

            return node.GetValue<T>();
        }

        /// <summary>
        /// Gets a value, returning a fallback if the key is missing.
        /// </summary>
        public T GetOrDefault<T>(string keyPath, T fallback)
        {
            try
            {
                JsonNode? node = Traverse(keyPath);
                if (node is null) return fallback;
                return node.GetValue<T>();
            }
            catch
            {
                return fallback;
            }
        }

        /// <summary>
        /// Returns true if the key path exists in the config.
        /// </summary>
        public bool Has(string keyPath) => Traverse(keyPath) is not null;

        /// <summary>
        /// Deserializes a section of the config into a typed object.
        /// e.g. GetSection<DatabaseConfig>("Database")
        /// </summary>
        public T? GetSection<T>(string keyPath)
        {
            JsonNode? node = Traverse(keyPath);
            if (node is null) return default;

            return node.Deserialize<T>();
        }

        /// <summary>
        /// Reloads the config from disk.
        /// </summary>
        public ConfigReader Reload() => new ConfigReader(_filePath);

        private JsonNode? Traverse(string keyPath)
        {
            JsonNode? current = _root;
            foreach (string key in keyPath.Split('.'))
            {
                if (current is JsonObject obj && obj.TryGetPropertyValue(key, out JsonNode? next))
                    current = next;
                else
                    return null;
            }
            return current;
        }
    }
}
