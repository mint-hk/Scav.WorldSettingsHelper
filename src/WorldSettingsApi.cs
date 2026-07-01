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
            ValidateNewSetting(key, label);
            Definitions[key] = new WorldSettingDefinition(key, label, WorldSettingKind.Bool, defaultValue);
        }

        public static void AddFloat(string key, string label, float defaultValue, float min, float max, bool wholeNumbers, string postfix)
        {
            ValidateNewSetting(key, label);
            if (min > max) throw new ArgumentException("Minimum value cannot be greater than maximum value.", nameof(min));
            Definitions[key] = new WorldSettingDefinition(key, label, WorldSettingKind.Float, Mathf.Clamp(defaultValue, min, max), min, max, wholeNumbers, postfix);
        }

        public static void AddDropdown(string key, string label, IEnumerable<string> options, int defaultIndex)
        {
            ValidateNewSetting(key, label);
            var optionList = new List<string>(options ?? Array.Empty<string>());
            optionList.RemoveAll(string.IsNullOrWhiteSpace);
            if (optionList.Count == 0) throw new ArgumentException("Dropdown settings require at least one non-empty option.", nameof(options));
            Definitions[key] = new WorldSettingDefinition(key, label, WorldSettingKind.Dropdown, Mathf.Clamp(defaultIndex, 0, optionList.Count - 1), options: optionList);
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

        private static void ValidateNewSetting(string key, string label)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Setting key cannot be empty.", nameof(key));
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Setting label cannot be empty.", nameof(label));
            if (Definitions.ContainsKey(key)) throw new InvalidOperationException($"A world setting with key '{key}' is already registered.");
        }

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
            EnsureDefaults(settings);
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
