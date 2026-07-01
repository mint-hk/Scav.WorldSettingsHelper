using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scav.WorldSettingsHelper
{
    internal static class WorldSettingsMenuRuntime
    {
        public static void AddRowsToMenu(PreRunScript menu)
        {
            if (menu == null || menu.runSettingObjects == null || menu.runSettingObjects.Count == 0) return;
            WorldSettingsApi.EnsureDefaults(menu.runSettings);
            foreach (var definition in WorldSettingsApi.AllDefinitions)
                AddRow(menu, WorldSettingsApi.CreateRunSetting(definition));
            RepositionCustomRows(menu);
            menu.UpdateAllSettingDisplays();
        }

        public static void RepairCustomRows(PreRunScript menu)
        {
            if (menu == null || menu.runSettingObjects == null) return;
            foreach (var row in menu.runSettingObjects)
            {
                if (row == null || row.associated == null) continue;
                if (WorldSettingsApi.IsCustomKey(row.associated.name))
                    RepairCustomRowUi(row, row.associated);
            }
            RepositionCustomRows(menu);
        }

        private static void AddRow(PreRunScript menu, RunSetting setting)
        {
            if (HasRow(menu, setting.name)) return;
            var template = FindTemplate(menu, setting);
            if (template == null)
            {
                WorldSettingsHelperPlugin.Log?.LogWarning($"Could not add custom world setting '{setting.name}' because no compatible vanilla setting row was found.");
                return;
            }
            var parent = menu.customSettingContent != null ? menu.customSettingContent : template.transform.parent;
            var cloneObject = Object.Instantiate(template.gameObject, parent);
            cloneObject.name = "Scav.WorldSettingsHelper_" + setting.name;
            cloneObject.transform.SetAsLastSibling();
            var row = cloneObject.GetComponent<RunSettingDisplay>();
            if (row == null) return;
            ClearClonedListeners(row);
            ResetDisplay(row);
            row.SetAssociated(setting);
            menu.runSettingObjects.Add(row);
            RepairCustomRowUi(row, setting);
        }

        private static RunSettingDisplay FindTemplate(PreRunScript menu, RunSetting setting)
        {
            foreach (var row in menu.runSettingObjects)
            {
                if (row == null || row.associated == null) continue;
                if (setting is RunSettingBool && row.associated is RunSettingBool) return row;
                if (setting is RunSettingDropdown && row.associated is RunSettingDropdown) return row;
                if (setting is RunSettingFloat && row.associated is RunSettingFloat) return row;
            }
            return menu.runSettingObjects.Count > 0 ? menu.runSettingObjects[menu.runSettingObjects.Count - 1] : null;
        }

        private static void ResetDisplay(RunSettingDisplay row)
        {
            typeof(RunSettingDisplay).GetField("didInitialize", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.SetValue(row, false);
            typeof(RunSettingDisplay).GetField("settingType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.SetValue(row, null);
        }

        private static void ClearClonedListeners(RunSettingDisplay row)
        {
            foreach (var toggle in row.GetComponentsInChildren<Toggle>(true)) toggle.onValueChanged.RemoveAllListeners();
            foreach (var slider in row.GetComponentsInChildren<Slider>(true)) slider.onValueChanged.RemoveAllListeners();
            foreach (var dropdown in row.GetComponentsInChildren<TMP_Dropdown>(true)) dropdown.onValueChanged.RemoveAllListeners();
        }

        private static bool HasRow(PreRunScript menu, string name)
        {
            foreach (var row in menu.runSettingObjects)
                if (row != null && row.associated != null && row.associated.name == name) return true;
            return false;
        }

        private static void RepairCustomRowUi(RunSettingDisplay row, RunSetting setting)
        {
            var label = WorldSettingsApi.GetLabel(setting.name);
            foreach (var text in row.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (text == null || string.IsNullOrEmpty(text.text)) continue;
                if (text.text.StartsWith("runset") || text.text == setting.name)
                {
                    text.text = label;
                    break;
                }
            }

            if (setting is RunSettingDropdown)
            {
                var dropdown = row.GetComponentInChildren<TMP_Dropdown>(true);
                if (dropdown != null)
                {
                    var selectedValue = dropdown.value;
                    var options = WorldSettingsApi.GetOptions(setting.name);
                    dropdown.options = options.ConvertAll(option => new TMP_Dropdown.OptionData(option));
                    dropdown.SetValueWithoutNotify(Mathf.Clamp(selectedValue, 0, Mathf.Max(0, options.Count - 1)));
                    dropdown.RefreshShownValue();
                }
            }
        }

        private static void RepositionCustomRows(PreRunScript menu)
        {
            if (menu == null || menu.runSettingObjects == null) return;
            var customRows = new List<RunSettingDisplay>();
            var vanillaRects = new List<RectTransform>();
            var lowestBottom = float.MaxValue;
            foreach (var row in menu.runSettingObjects)
            {
                if (row == null || row.associated == null) continue;
                var rect = row.GetComponent<RectTransform>();
                if (rect == null) continue;
                if (WorldSettingsApi.IsCustomKey(row.associated.name)) customRows.Add(row);
                else
                {
                    lowestBottom = Mathf.Min(lowestBottom, GetBottomEdge(rect));
                    vanillaRects.Add(rect);
                }
            }
            if (customRows.Count == 0) return;
            if (lowestBottom == float.MaxValue) lowestBottom = 0f;
            var borderOffset = GetVanillaBorderOffset(vanillaRects);
            var currentTop = lowestBottom + borderOffset;
            foreach (var row in customRows)
            {
                var rect = row.GetComponent<RectTransform>();
                if (rect == null) continue;
                var pos = rect.anchoredPosition;
                pos.y = currentTop - (1f - rect.pivot.y) * rect.rect.height;
                rect.anchoredPosition = pos;
                row.transform.SetAsLastSibling();
                currentTop = GetBottomEdge(rect) + borderOffset;
            }
            if (menu.customSettingContent != null)
            {
                var size = menu.customSettingContent.sizeDelta;
                size.y = Mathf.Max(size.y, Mathf.Abs(currentTop) + GetRowHeight(customRows[customRows.Count - 1]));
                menu.customSettingContent.sizeDelta = size;
            }
        }

        private static float GetRowHeight(RunSettingDisplay row)
        {
            var rect = row != null ? row.GetComponent<RectTransform>() : null;
            return rect == null || rect.rect.height <= 0f ? 70f : rect.rect.height;
        }

        private static float GetBottomEdge(RectTransform rect) => rect.anchoredPosition.y - rect.pivot.y * rect.rect.height;
        private static float GetTopEdge(RectTransform rect) => rect.anchoredPosition.y + (1f - rect.pivot.y) * rect.rect.height;

        private static float GetVanillaBorderOffset(List<RectTransform> rects)
        {
            if (rects == null || rects.Count < 2) return 0f;
            rects.Sort((a, b) => GetTopEdge(b).CompareTo(GetTopEdge(a)));
            var bestOffset = 0f;
            var bestAbs = float.MaxValue;
            for (var i = 1; i < rects.Count; i++)
            {
                var offset = GetTopEdge(rects[i]) - GetBottomEdge(rects[i - 1]);
                var abs = Mathf.Abs(offset);
                if (abs < bestAbs && abs <= 12f)
                {
                    bestAbs = abs;
                    bestOffset = offset;
                }
            }
            return bestOffset;
        }
    }

    [HarmonyPatch(typeof(PreRunScript), "Start")]
    internal static class PreRunScriptStartPatch
    {
        private static void Postfix(PreRunScript __instance)
        {
            WorldSettingsMenuRuntime.AddRowsToMenu(__instance);
            WorldSettingsMenuRuntime.RepairCustomRows(__instance);
        }
    }

    [HarmonyPatch(typeof(PreRunScript), "ApplyPreset")]
    internal static class PreRunScriptApplyPresetPatch
    {
        private static void Prefix(PreRunScript __instance)
        {
            WorldSettingsApi.EnsureDefaults(__instance?.runSettings);
        }

        private static void Postfix(PreRunScript __instance)
        {
            WorldSettingsApi.EnsureDefaults(__instance?.runSettings);
            __instance?.UpdateAllSettingDisplays();
            WorldSettingsMenuRuntime.RepairCustomRows(__instance);
        }
    }

    [HarmonyPatch(typeof(PreRunScript), "UpdateAllSettingDisplays")]
    internal static class PreRunScriptUpdateAllSettingDisplaysPatch
    {
        private static void Prefix(PreRunScript __instance)
        {
            WorldSettingsApi.EnsureDefaults(__instance?.runSettings);
        }

        private static void Postfix(PreRunScript __instance)
        {
            WorldSettingsMenuRuntime.RepairCustomRows(__instance);
        }
    }

    [HarmonyPatch(typeof(RunSettingDisplay), "UpdateSettingDisplay")]
    internal static class RunSettingDisplayUpdateSettingDisplayPatch
    {
        private static void Prefix(Dictionary<string, object> settings)
        {
            WorldSettingsApi.EnsureDefaults(settings);
        }
    }

    [HarmonyPatch(typeof(RunSettingDisplay), "UpdateSetting")]
    internal static class RunSettingDisplayUpdateSettingPatch
    {
        private static void Prefix(Dictionary<string, object> settings)
        {
            WorldSettingsApi.EnsureDefaults(settings);
        }
    }
}
