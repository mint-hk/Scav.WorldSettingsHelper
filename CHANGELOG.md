# Changelog

## 0.1.1

- Adds validation for custom setting registration.
- Makes registered setting definitions immutable to consumers.
- Logs a warning when a compatible vanilla setting row cannot be found.
- Documents the required BepInEx dependency declaration for consuming mods.

## 0.1.0

- Initial release build.
- Adds `WorldSettingsApi` for bool, float, and dropdown world settings.
- Adds runtime integration with the existing custom world settings menu.
- Adds default-value injection to avoid missing-key crashes.
