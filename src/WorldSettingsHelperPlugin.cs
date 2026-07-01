using BepInEx;
using HarmonyLib;

namespace Scav.WorldSettingsHelper
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class WorldSettingsHelperPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.mint-hk.scav.worldsettingshelper";
        public const string PluginName = "Scav.WorldSettingsHelper";
        public const string PluginVersion = "0.1.0";

        private Harmony _harmony;

        private void Awake()
        {
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            Logger.LogInfo("Scav.WorldSettingsHelper loaded.");
        }
    }
}
