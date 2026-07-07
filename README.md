# WidBar Widget Template

Build Windows 11 taskbar widgets for [WidBar](https://apps.microsoft.com/detail/9PKLDNM83TP9)
with WinUI 3, C#, and the Windows App SDK.
<p align="center">
  <img src="https://github.com/user-attachments/assets/9e463b8d-4692-4cd8-a128-fab4a3474d33" width="200" height="240" alt="Screenshot 1">
  <img src="https://github.com/user-attachments/assets/0c1ef7af-9be5-4e89-867e-7ba89517d4bd" width="200" height="198" alt="Screenshot 2">
  <img src="https://github.com/user-attachments/assets/934b6043-616d-4981-abad-fa721e63d917" width="200" height="262" alt="Screenshot 3">
  <img src="https://github.com/user-attachments/assets/95e29b25-453c-4ce8-adf7-f4350a26170d" width="200" height="227" alt="Screenshot 4">
</p>
### Discord channel: https://discord.com/invite/JxyNUmznt

This repository is a `dotnet new` template that scaffolds a working widget: the
isolated plugin process plus the MSIX packaging project that Windows needs to
discover it. It targets the [`WidBar.SDK`](https://www.nuget.org/packages/WidBar.SDK)
NuGet package (currently 1.2.0).

This template and the `WidBar.SDK` package are for people building widgets. To
just use widgets, install WidBar from the Microsoft Store.

## Documentation

The full developer guide (getting started, the plugin contract, the three UI
surfaces, packaging, publishing, and debugging) is in the **[Wiki](../../wiki)**.
The same pages ship in this repo under [`wiki/`](wiki/).

## What you can build

A widget provides up to three independent WinUI 3 surfaces:

- Taskbar preview: compact, transparent WinUI content shown in a free space on
  the Windows taskbar.
- Flyout: a rich interactive window opened when the user clicks the preview.
- Settings: optional configuration UI, hosted by WidBar with Save and Cancel.

You return ordinary WinUI `UIElement`s. WidBar and the SDK handle taskbar
integration, hosting, per-monitor DPI, settings persistence, process monitoring
and crash recovery, none of which you write.

## Prerequisites

- Windows 11, with Developer Mode enabled (for local deploy).
- .NET 8 SDK and the Windows App SDK, or Visual Studio 2022+ with the *Windows
  application development* workload.

## Get started

```powershell
# install this template (once)
dotnet new install .

# create your widget
dotnet new widbar-widget -n Contoso.Weather `
    --PluginId com.contoso.weather --DisplayName "Contoso Weather"
```

This generates two projects:

```text
Contoso.Weather.ExtensionApp/   your widget: plugin class, preview, flyout, settings
Contoso.Weather (Package)/       MSIX packaging project, what you publish
```

Build the packaging project (`Debug | x64`), deploy it (Visual Studio
*Build > Deploy*, or register the loose MSIX layout with Developer Mode on), then
start WidBar and drag your widget onto the taskbar. See
[Getting Started](wiki/Getting-Started.md) for the full loop.

## The plugin in 30 seconds

```csharp
public sealed class WeatherPlugin : WidgetPluginBase, IConfigurableWidgetPlugin
{
    public override string Id   => "com.contoso.weather";
    public override string Name => "Weather";
    public override WidgetCategory Category => WidgetCategory.Information;

    public override UIElement? CreatePreviewContent() => new WeatherPreviewView();
    public override UIElement? CreateFlyoutContent()  => new WeatherFlyoutView();
    public UIElement? CreateSettingsContent(IWidgetSettingsContext ctx) =>
        new WeatherSettingsView(ctx);
}
```

The catalog manifest (`plugin.json`) is generated at build time from the
`WidBarPlugin*` properties in the ExtensionApp `.csproj`, so there is nothing to
keep in sync by hand.

## Publish

1. Submit the packaging project's `.msixupload` to the Microsoft Store.
2. Optionally list your widget in WidBar's in-app showcase by adding an entry to
   [andelby/winbar-showcase](https://github.com/andelby/winbar-showcase).

See [Packaging and Publishing](wiki/Packaging-and-Publishing.md).

## Links

- SDK package: <https://www.nuget.org/packages/WidBar.SDK>
- Wiki: [developer documentation](../../wiki)
- WidBar on the Store: <https://apps.microsoft.com/detail/9PKLDNM83TP9>

## License

MIT, see [LICENSE](LICENSE).
