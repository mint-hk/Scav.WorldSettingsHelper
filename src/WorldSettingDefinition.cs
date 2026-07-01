using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Scav.WorldSettingsHelper
{
    public enum WorldSettingKind
    {
        Bool,
        Float,
        Dropdown
    }

    public sealed class WorldSettingDefinition
    {
        internal WorldSettingDefinition(string key, string label, WorldSettingKind kind, object defaultValue, float min = 0f, float max = 0f, bool wholeNumbers = false, string postfix = null, IEnumerable<string> options = null)
        {
            Key = key;
            Label = label;
            Kind = kind;
            DefaultValue = defaultValue;
            Min = min;
            Max = max;
            WholeNumbers = wholeNumbers;
            Postfix = postfix ?? string.Empty;
            Options = new ReadOnlyCollection<string>(new List<string>(options ?? new string[0]));
        }

        public string Key { get; }
        public string Label { get; }
        public WorldSettingKind Kind { get; }
        public object DefaultValue { get; }
        public float Min { get; }
        public float Max { get; }
        public bool WholeNumbers { get; }
        public string Postfix { get; }
        public IReadOnlyList<string> Options { get; }
    }
}
