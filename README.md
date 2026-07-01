# Scav.WorldSettingsHelper

Standalone BepInEx + Harmony helper library for adding custom world/run settings to the existing Casualties Unknown custom settings menu.

## Requirements

- Casualties Unknown / Scav Prototype
- BepInEx 5.x

For building from source:

- .NET SDK / MSBuild
- .NET Framework 4.8 Developer Pack

## Build

```powershell
dotnet msbuild Scav.WorldSettingsHelper.csproj /p:Configuration=Release /p:GameDir="C:\Path\To\Casualties Unknown Demo"
```

Install:

```text
<Game>/BepInEx/plugins/Scav.WorldSettingsHelper.dll
```

Mods using this helper should reference `Scav.WorldSettingsHelper.dll` and require it to be installed in `BepInEx/plugins`.

## API

Register settings during your plugin startup:

```csharp
WorldSettingsApi.AddBool(string key, string label, bool defaultValue);
WorldSettingsApi.AddFloat(string key, string label, float defaultValue, float min, float max, bool wholeNumbers, string postfix);
WorldSettingsApi.AddDropdown(string key, string label, IEnumerable<string> options, int defaultIndex);
```

Read settings after a world is loaded:

```csharp
WorldSettingsApi.GetBool(string key, bool fallback);
WorldSettingsApi.GetFloat(string key, float fallback);
WorldSettingsApi.GetInt(string key, int fallback);
```

## Example

```csharp
using BepInEx;
using Scav.WorldSettingsHelper;

[BepInPlugin("com.example.testmod", "Test Mod", "1.0.0")]
public class TestModPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        WorldSettingsApi.AddBool("example_enabled", "Enable Example", false);
        WorldSettingsApi.AddDropdown("example_mode", "Example Mode", new[] { "Default", "Hardcore", "Chaos" }, 0);
        WorldSettingsApi.AddFloat("example_interval", "Example Interval", 10f, 1f, 30f, true, " min");
    }
}
```

Reading values:

```csharp
bool enabled = WorldSettingsApi.GetBool("example_enabled", false);
int mode = WorldSettingsApi.GetInt("example_mode", 0);
float interval = WorldSettingsApi.GetFloat("example_interval", 10f);
```

## Notes

- The helper clones existing vanilla setting rows after the custom settings menu is built.
- It does not patch the game's static run settings registry early.
- It keeps default values present in setting dictionaries to avoid missing-key crashes.
- Dropdown values are returned as the game's stored integer index.
