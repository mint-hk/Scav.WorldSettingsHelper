using System.Collections.Generic;

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
        public string Key { get; set; }
        public string Label { get; set; }
        public WorldSettingKind Kind { get; set; }
        public object DefaultValue { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public bool WholeNumbers { get; set; }
        public string Postfix { get; set; }
        public List<string> Options { get; set; }
    }
}
