# WidBar Widget Template

Build Windows 11 taskbar widgets for [WidBar](https://apps.microsoft.com/detail/9PKLDNM83TP9) with WinUI 3, C#, and the Windows App SDK.

WidBar widgets can be built as standalone widget packages, or integrated directly into an existing WinUI app by referencing the `WidBar.SDK` NuGet package and returning normal WinUI `UIElement`s for the taskbar preview, flyout, and settings surfaces.

<p align="center">
  <img src="https://github.com/user-attachments/assets/9e463b8d-4692-4cd8-a128-fab4a3474d33" width="200" height="240" alt="Screenshot 1">
  <img src="https://github.com/user-attachments/assets/0c1ef7af-9be5-4e89-867e-7ba89517d4bd" width="200" height="198" alt="Screenshot 2">
  <img src="https://github.com/user-attachments/assets/934b6043-616d-4981-abad-fa721e63d917" width="200" height="262" alt="Screenshot 3">
  <img src="https://github.com/user-attachments/assets/95e29b25-453c-4ce8-adf7-f4350a26170d" width="200" height="227" alt="Screenshot 4">
</p>

### Discord channel: https://discord.com/invite/JxyNUmznt

This repository is a `dotnet new` template that scaffolds a working WidBar widget, including the isolated plugin process and the MSIX packaging project needed for Windows to discover it.

It targets the [`WidBar.SDK`](https://www.nuget.org/packages/WidBar.SDK) NuGet package.

This template is the fastest way to start a new standalone widget. If you already have a WinUI app, you can also use the SDK directly inside your existing app and expose WidBar surfaces from there, without creating a separate widget project from this template.

This template and the `WidBar.SDK` package are for developers building widgets. To just use widgets, install WidBar from the Microsoft Store.

## Documentation

The full developer guide, including getting started, the plugin contract, UI surfaces, packaging, publishing, debugging, and existing app integration, is in the **[Wiki](../../wiki)**.

The same pages ship in this repo under [`wiki/`](wiki/).

## What you can build

A WidBar widget can provide up to three independent WinUI 3 surfaces:

* Taskbar preview: compact, transparent WinUI content shown in a free space on the Windows taskbar.
* Flyout: a rich interactive window opened when the user clicks the preview.
* Settings: optional configuration UI, hosted by WidBar with Save and Cancel.

You return ordinary WinUI `UIElement`s. WidBar and the SDK handle taskbar integration, hosting, per-monitor DPI, settings persistence, process monitoring, and crash recovery.

This makes WidBar useful for more than traditional widgets. You can build small taskbar companions for existing apps, status panels, quick controls, live previews, dashboards, launchers, timers, media controls, monitoring tools, or any other compact experience that belongs close to the taskbar.

## Two ways to build

### 1. Create a standalone widget

Use this template when you want to create a dedicated widget package for WidBar.

```powershell
# install this template once
dotnet new install .

# create your widget
dotnet new widbar-widget -n Contoso.Weather `
    --PluginId com.contoso.weather --DisplayName "Contoso Weather"
```

This generates two projects:

```text
Contoso.Weather.ExtensionApp/    your widget: plugin class, preview, flyout, settings
Contoso.Weather (Package)/       MSIX packaging project, what you publish
```

Build the packaging project, deploy it, start WidBar, and drag your widget onto the taskbar.

### 2. Integrate WidBar into an existing app

Use the SDK directly when you already have a WinUI app and want to add a taskbar companion experience.

In this case, you do not need to create a new widget from the template. Add the `WidBar.SDK` NuGet package to your existing app, implement the plugin contract, and return your own WinUI `UIElement`s for the preview and flyout.

This is useful when your app already has its own data, services, settings, or background logic, and you want to expose part of that experience on the Windows taskbar.

Examples:

* A music app can expose playback controls on the taskbar.
* A productivity app can expose timers, tasks, or quick actions.
* A monitoring app can expose live status and detailed flyouts.
* A finance app can expose prices, watchlists, or alerts.
* A communication app can expose presence, unread counts, or shortcuts.

## Prerequisites

* Windows 11, with Developer Mode enabled for local deploy.
* .NET 8 SDK and the Windows App SDK, or Visual Studio 2022+ with the Windows application development workload.

## The plugin in 30 seconds

```csharp
public sealed class WeatherPlugin : WidgetPluginBase, IConfigurableWidgetPlugin
{
    public override string Id   => "com.contoso.weather";
    public override string Name => "Weather";
    public override WidgetCategory Category => WidgetCategory.Information;

    public override UIElement? CreatePreviewContent() => new WeatherPreviewView();

    public override UIElement? CreateFlyoutContent() => new WeatherFlyoutView();

    public UIElement? CreateSettingsContent(IWidgetSettingsContext ctx) =>
        new WeatherSettingsView(ctx);
}
```

The catalog manifest (`plugin.json`) is generated at build time from the `WidBarPlugin*` properties in the ExtensionApp `.csproj`, so there is nothing to keep in sync by hand.

When integrating into an existing app, the same idea applies: your app provides the plugin class and WinUI surfaces, while WidBar hosts them on the taskbar.

## Publish

For standalone widgets:

1. Submit the packaging project's `.msixupload` to the Microsoft Store.
2. Optionally list your widget in WidBar's in-app showcase by adding an entry to [andelby/winbar-showcase](https://github.com/andelby/winbar-showcase).

For existing app integrations, publish your app normally. WidBar can discover and use the surfaces exposed through the SDK when your app is installed and correctly configured.

See [Packaging and Publishing](wiki/Packaging-and-Publishing.md).

## Links

* SDK package: https://www.nuget.org/packages/WidBar.SDK
* Wiki: [developer documentation](../../wiki)
* WidBar on the Store: https://apps.microsoft.com/detail/9PKLDNM83TP9

## License

MIT, see [LICENSE](LICENSE).
