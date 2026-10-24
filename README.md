# WidBar Widget Templates

Build Windows 11 taskbar widgets for
[WidBar](https://apps.microsoft.com/detail/9PKLDNM83TP9) with WinUI 3, C# and the
Windows App SDK.

A widget is normal WinUI content: a small preview on the taskbar, a flyout that
opens above it and an optional settings page. `WidBar.SDK` 2.2 does the rest:
placement, hover and clicks, the flyout window, per-instance settings, themes,
Smart Stacks and crash recovery.

<p align="center">
  <img src="https://github.com/user-attachments/assets/9e463b8d-4692-4cd8-a128-fab4a3474d33" width="200" height="240" alt="Screenshot 1">
  <img src="https://github.com/user-attachments/assets/0c1ef7af-9be5-4e89-867e-7ba89517d4bd" width="200" height="198" alt="Screenshot 2">
  <img src="https://github.com/user-attachments/assets/934b6043-616d-4981-abad-fa721e63d917" width="200" height="262" alt="Screenshot 3">
  <img src="https://github.com/user-attachments/assets/95e29b25-453c-4ce8-adf7-f4350a26170d" width="200" height="227" alt="Screenshot 4">
</p>

Developer community: https://discord.com/invite/JxyNUmznt

## Templates

| Template | Folder | Creates |
| --- | --- | --- |
| `widbar-widget` | `templates/standalone/` | A widget with its own MSIX package and Store listing |
| `widbar-companion` | `templates/companion/` | A widget exe to add to a packaged app you already ship |

Install them from this folder, then create a widget:

```powershell
dotnet new install .

dotnet new widbar-widget -n Contoso.Weather `
    --PluginId com.contoso.weather `
    --DisplayName "Contoso Weather"
```

## Documentation

The developer guide lives in [`docs/`](docs/Home.md) and is published to the
[wiki](https://github.com/andelby/widbar-widget-template/wiki) on every change.

| Start with | Then | Look up |
| --- | --- | --- |
| [Quick start](docs/Quick-Start.md) | [Taskbar preview](docs/Taskbar-Preview.md) | [API reference](docs/API-Reference.md) |
| [Lifecycle](docs/Lifecycle.md) | [Flyout](docs/Flyout.md) | [Debugging](docs/Debugging.md) |
| | [Settings](docs/Settings.md) | [FAQ](docs/FAQ.md) |
| | [Packaging and publishing](docs/Packaging-and-Publishing.md) | [Changelog](docs/Changelog.md) |
| | [Companion widgets](docs/Companion-Widgets.md) | |

## What you need

- Windows 11 with Developer Mode on
- WidBar from the Microsoft Store
- Visual Studio 2026 with the WinUI application development workload, or the .NET 10 SDK and MSBuild

## The plugin in 30 seconds

```csharp
public sealed class WeatherPlugin : WidgetPluginBase, IConfigurableWidgetPlugin
{
    public override string Id => "com.contoso.weather";
    public override string Name => "Weather";
    public override int PreviewLogicalWidth => 180;

    public override UIElement? CreatePreviewContent() => new WeatherPreviewView();

    public override UIElement? CreateFlyoutContent() => new WeatherFlyoutView();

    public UIElement? CreateSettingsContent(IWidgetSettingsContext context) =>
        new WeatherSettingsView(context);
}
```

The catalog details (description, category, version) come from the
`WidBarPlugin*` properties in the project file, not from the plugin class.

## Links

- SDK package: https://www.nuget.org/packages/WidBar.SDK
- WidBar on the Microsoft Store: https://apps.microsoft.com/detail/9PKLDNM83TP9
- WidBar, issues and the widget catalog: https://github.com/andelby/widbar

## License

MIT, see [LICENSE](LICENSE).
