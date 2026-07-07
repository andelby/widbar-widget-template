# WidBar Widget Templates

Build Windows 11 taskbar widgets for [WidBar](https://apps.microsoft.com/detail/9PKLDNM83TP9) with WinUI 3, C#, and the Windows App SDK.

WidBar widgets return normal WinUI `UIElement`s for the taskbar preview, flyout, and settings surfaces. The `WidBar.SDK` NuGet package handles the host connection, settings draft flow, taskbar placement, process monitoring, logs, and recovery behavior.

<p align="center">
  <img src="https://github.com/user-attachments/assets/9e463b8d-4692-4cd8-a128-fab4a3474d33" width="200" height="240" alt="Screenshot 1">
  <img src="https://github.com/user-attachments/assets/0c1ef7af-9be5-4e89-867e-7ba89517d4bd" width="200" height="198" alt="Screenshot 2">
  <img src="https://github.com/user-attachments/assets/934b6043-616d-4981-abad-fa721e63d917" width="200" height="262" alt="Screenshot 3">
  <img src="https://github.com/user-attachments/assets/95e29b25-453c-4ce8-adf7-f4350a26170d" width="200" height="227" alt="Screenshot 4">
</p>

### Discord channel: https://discord.com/invite/JxyNUmznt

This repo contains two `dotnet new` templates:

```text
templates/standalone/   widbar-widget: a standalone widget package
templates/companion/    widbar-companion: a widget exe for an existing app package
```

## Install the templates

From this repository folder:

```powershell
dotnet new install .
```

## Create a standalone widget

Use this when you want a dedicated Microsoft Store package for your widget.

```powershell
dotnet new widbar-widget -n Contoso.Weather `
    --PluginId com.contoso.weather `
    --DisplayName "Contoso Weather"
```

This creates:

```text
Contoso.Weather.ExtensionApp/    widget process and plugin code
Contoso.Weather (Package)/       MSIX packaging project to publish
```

## Create a companion widget

Use this when you already have a packaged WinUI app and want to ship a WidBar widget inside the same MSIX.

```powershell
dotnet new widbar-companion -n Contoso.App `
    --PluginId com.contoso.app.companion `
    --DisplayName "Contoso App"
```

This creates:

```text
Contoso.App.Widget/              tiny widget exe to add to your app package
```

Add the generated project to your existing solution, reference it from your `.wapproj`, link its generated `obj\widbar\plugin.json` into `Public\plugin.json`, and add a second hidden `<Application>` entry with the `com.widbar.widget` AppExtension.

The full walkthrough is in [Companion Widgets](wiki/Companion-Widgets.md).

## What a widget can provide

A WidBar widget can expose three WinUI surfaces:

* Taskbar preview: compact content shown in a free space on the Windows taskbar.
* Flyout: a rich interactive window opened when the user clicks the preview.
* Settings: optional configuration UI hosted by WidBar with Save and Cancel.

Examples include performance meters, media controls, timers, app companions, launchers, dashboards, monitoring tools, and quick actions.

## Documentation

The full developer guide is in the [Wiki](../../wiki). The same pages ship in this repo under [`wiki/`](wiki/).

Start here:

* [Getting Started](wiki/Getting-Started.md)
* [Plugin Contract](wiki/Plugin-Contract.md)
* [Preview, Flyout and Settings](wiki/Preview-Flyout-Settings.md)
* [Companion Widgets](wiki/Companion-Widgets.md)
* [Packaging and Publishing](wiki/Packaging-and-Publishing.md)

## Prerequisites

* Windows 11.
* WidBar installed from the Microsoft Store.
* .NET 8 SDK.
* Visual Studio 2022+ with the Windows application development workload, or equivalent Windows App SDK tooling.
* Developer Mode enabled for local package deployment.

## The plugin in 30 seconds

```csharp
public sealed class WeatherPlugin : WidgetPluginBase, IConfigurableWidgetPlugin
{
    public override string Id => "com.contoso.weather";
    public override string Name => "Weather";
    public override WidgetCategory Category => WidgetCategory.Information;

    public override UIElement? CreatePreviewContent() => new WeatherPreviewView();

    public override UIElement? CreateFlyoutContent() => new WeatherFlyoutView();

    public UIElement? CreateSettingsContent(IWidgetSettingsContext context) =>
        new WeatherSettingsView(context);
}
```

The catalog manifest (`plugin.json`) is generated at build time from the `WidBarPlugin*` properties in the widget project file.

## Links

* SDK package: https://www.nuget.org/packages/WidBar.SDK
* WidBar on the Store: https://apps.microsoft.com/detail/9PKLDNM83TP9
* Widget showcase: https://andelby.github.io/winbar-showcase/

## License

MIT, see [LICENSE](LICENSE).
