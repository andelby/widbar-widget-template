# WidBar Widget Templates

Build Windows 11 taskbar widgets for
[WidBar](https://apps.microsoft.com/detail/9PKLDNM83TP9) with WinUI 3, C# and the
Windows App SDK.

These templates use the official `WidBar.SDK` 2.0 contract. A widget provides
normal WinUI `UIElement` content for the taskbar preview, flyout and optional
settings page. WidBar handles discovery, placement, per-instance settings,
automatic light and dark themes, smart stacks, logs and process recovery.

<p align="center">
  <img src="https://github.com/user-attachments/assets/9e463b8d-4692-4cd8-a128-fab4a3474d33" width="200" height="240" alt="Screenshot 1">
  <img src="https://github.com/user-attachments/assets/0c1ef7af-9be5-4e89-867e-7ba89517d4bd" width="200" height="198" alt="Screenshot 2">
  <img src="https://github.com/user-attachments/assets/934b6043-616d-4981-abad-fa721e63d917" width="200" height="262" alt="Screenshot 3">
  <img src="https://github.com/user-attachments/assets/95e29b25-453c-4ce8-adf7-f4350a26170d" width="200" height="227" alt="Screenshot 4">
</p>

Developer community: https://discord.com/invite/JxyNUmznt

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

Use this when you already have a packaged WinUI app and want to ship a WidBar
widget inside the same MSIX. It is a good fit when your app already owns the
data or services that the widget needs.

Examples:

- A music app can expose playback controls on the taskbar.
- A productivity app can expose timers, tasks, or quick actions.
- A monitoring app can expose live status and detailed flyouts.
- A finance app can expose prices, watchlists, or alerts.
- A communication app can expose presence, unread counts, or shortcuts.

```powershell
dotnet new widbar-companion -n Contoso.App `
    --PluginId com.contoso.app.companion `
    --DisplayName "Contoso App"
```

This creates:

```text
Contoso.App.Widget/              tiny widget exe to add to your app package
```

Add the generated project to your existing solution, reference it from your
`.wapproj`, link its generated `obj\widbar\plugin.json` into
`Public\plugin.json`, and add a second hidden `<Application>` entry with the
`com.widbar.widget` AppExtension.

The full walkthrough is in [Companion Widgets](wiki/Companion-Widgets.md).

## What a widget can provide

A WidBar widget can expose three WinUI surfaces:

* Taskbar preview: compact content shown in a free space on the Windows taskbar.
* Flyout: a rich interactive window opened when the user clicks the preview.
* Settings: optional configuration UI hosted by WidBar with Save and Cancel.

The preview can contain live controls and lightweight animations. Flyouts can
host richer controls, pickers and third-party UI libraries. Standard WinUI
theme resources update automatically with the Windows theme.

Widgets can also take part in smart stacks. They can pause preview-only work
while another stack member is visible and request attention when an important
event occurs.

## Documentation

The full developer guide is in the
[GitHub wiki](https://github.com/andelby/widbar-widget-template/wiki). The
source pages are also available in [`wiki/`](wiki/).

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
* Visual Studio 2022 or later with the Windows application development workload,
  or equivalent Windows App SDK tooling.
* Developer Mode enabled for local package deployment.

## The plugin in 30 seconds

```csharp
public sealed class WeatherPlugin : WidgetPluginBase, IConfigurableWidgetPlugin
{
    public override string Id => "com.contoso.weather";
    public override string Name => "Weather";
    public override int PreviewLogicalWidth => 180;
    public override WidgetFlyoutBackdrop FlyoutBackdrop =>
        WidgetFlyoutBackdrop.Mica;

    public override UIElement? CreatePreviewContent() => new WeatherPreviewView();

    public override UIElement? CreateFlyoutContent() => new WeatherFlyoutView();

    public UIElement? CreateSettingsContent(IWidgetSettingsContext context) =>
        new WeatherSettingsView(context);
}
```

The catalog manifest is generated at build time from the `WidBarPlugin*`
properties in the widget project file. Description and category belong there,
not on the runtime plugin class.

## Links

* SDK package: https://www.nuget.org/packages/WidBar.SDK
* WidBar on the Store: https://apps.microsoft.com/detail/9PKLDNM83TP9
* Widget showcase: [https://andelby.github.io/widbar/](https://github.com/andelby/widbar)

## License

MIT, see [LICENSE](LICENSE).
