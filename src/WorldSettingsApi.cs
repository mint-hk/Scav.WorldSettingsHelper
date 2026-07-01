using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scav.WorldSettingsHelper
{
    public static class WorldSettingsApi
    {
        private static readonly Dictionary<string, WorldSettingDefinition> Definitions = new Dictionary<string, WorldSettingDefinition>(StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyCollection<WorldSettingDefinition> AllDefinitions => Definitions.Values.ToList().AsReadOnly();

        public static void AddBool(string key, string label, bool defaultValue)
        {
            Definitions[key] = new WorldSettingDefinition { Key = key, Label = label, Kind = WorldSettingKind.Bool, DefaultValue = defaultValue };
        }

        public static void AddFloat(string key, string label, float defaultValue, float min, float max, bool wholeNumbers, string postfix)
        {
            Definitions[key] = new WorldSettingDefinition { Key = key, Label = label, Kind = WorldSettingKind.Float, DefaultValue = defaultValue, Min = min, Max = max, WholeNumbers = wholeNumbers, Postfix = postfix ?? string.Empty };
        }

        public static void AddDropdown(string key, string label, IEnumerable<string> options, int defaultIndex)
        {
            var optionList = new List<string>(options ?? Array.Empty<string>());
            Definitions[key] = new WorldSettingDefinition { Key = key, Label = label, Kind = WorldSettingKind.Dropdown, DefaultValue = Mathf.Clamp(defaultIndex, 0, Math.Max(0, optionList.Count - 1)), Options = optionList };
        }

        public static bool GetBool(string key, bool fallback)
        {
            return TryGetValue(key, out var value) ? ToBool(value, fallback) : fallback;
        }

        public static float GetFloat(string key, float fallback)
        {
            return TryGetValue(key, out var value) ? ToFloat(value, fallback) : fallback;
        }

        public static int GetInt(string key, int fallback)
        {
            return TryGetValue(key, out var value) ? ToInt(value, fallback) : fallback;
        }

        public static void EnsureDefaults(Dictionary<string, object> values)
        {
            if (values == null) return;
            foreach (var definition in Definitions.Values)
                if (!values.ContainsKey(definition.Key)) values[definition.Key] = definition.DefaultValue;
        }

        internal static bool IsCustomKey(string key) => key != null && Definitions.ContainsKey(key);
        internal static string GetLabel(string key) => Definitions.TryGetValue(key, out var definition) ? definition.Label : key;
        internal static List<string> GetOptions(string key) => Definitions.TryGetValue(key, out var definition) && definition.Options != null ? new List<string>(definition.Options) : new List<string>();

        internal static RunSetting CreateRunSetting(WorldSettingDefinition definition)
        {
            switch (definition.Kind)
            {
                case WorldSettingKind.Bool:
                    return new RunSettingBool(definition.Key);
                case WorldSettingKind.Dropdown:
                    return new RunSettingDropdown(definition.Key, definition.Options.ToArray());
                default:
                    return new RunSettingFloat(definition.Key) { limits = new RangeF(definition.Min, definition.Max), wholeNum = definition.WholeNumbers, postfix = definition.Postfix ?? string.Empty };
            }
        }

        private static bool TryGetValue(string key, out object value)
        {
            value = null;
            var settings = WorldGeneration.runSettings;
            return settings != null && settings.TryGetValue(key, out value);
        }

        private static bool ToBool(object value, bool fallback)
        {
            if (value is bool boolValue) return boolValue;
            return bool.TryParse(value?.ToString(), out var parsed) ? parsed : fallback;
        }

        private static float ToFloat(object value, float fallback)
        {
            if (value is float floatValue) return floatValue;
            if (value is int intValue) return intValue;
            return float.TryParse(value?.ToString(), out var parsed) ? parsed : fallback;
        }

        private static int ToInt(object value, int fallback)
        {
            if (value is int intValue) return intValue;
            if (value is float floatValue) return Mathf.RoundToInt(floatValue);
            return int.TryParse(value?.ToString(), out var parsed) ? parsed : fallback;
        }
    }
}
