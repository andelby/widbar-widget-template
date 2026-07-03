# The plugin contract

Everything you implement lives in the `WidBar.SDK` namespace. You can implement
`IWidgetPlugin` directly, but in practice you'll derive from `WidgetPluginBase`
and override the handful of members you actually care about. It gives you sane
defaults for everything else.

Here's a minimal but real plugin:

```csharp
public sealed class WeatherPlugin : WidgetPluginBase
{
    public override string Id   => "com.contoso.weather";
    public override string Name => "Weather";
    public override WidgetCategory Category => WidgetCategory.Information;
    public override int PreviewLogicalWidth => 160;

    public override async Task InitializeAsync(IWidgetContext context)
    {
        await base.InitializeAsync(context);   // stashes Context for you
        // Start your timer or load cached data here, then:
        // Context.RequestPreviewRefresh();
    }

    public override UIElement? CreatePreviewContent() => new WeatherPreview();
}
```

`Id` and `Name` are the only things `WidgetPluginBase` forces you to provide.
Everything below has a default.

## The members you'll touch

The identity properties (`Id`, `Name`, `Description`, `Version`, `Category`) show
up in WidBar's UI. `Category` is the `WidgetCategory` enum, defaulting to
`Utility`.

For layout, `PreviewLogicalWidth` is how wide you want your taskbar slot, in
logical (96-DPI) pixels, defaulting to 188. WidBar re-reads it after init and
after every settings change, so it's fine to make it depend on the user's
settings. `IsPreviewVisible` lets you hide the preview without tearing the
instance down, which is handy for "only show when there's something to show";
flip it and call `RequestPreviewRefresh()`. `FlyoutWidth` and `FlyoutHeight`
(390 by 480 by default) size the popup, and `FlyoutBackdrop` picks `Transparent`,
`Mica`, or `Acrylic`.

The methods are where the work happens:

| Method | When it runs | Default |
|--------|--------------|---------|
| `InitializeAsync(IWidgetContext)` | once, before anything is shown | stores `Context` |
| `CreatePreviewContent()` | to build the taskbar UI | returns `null` |
| `CreateFlyoutContent()` | when the user opens the popup (`null` means no popup) | returns `null` |
| `OnSettingsDraftChanged(json)` | on every edit in the settings UI | no-op |
| `DisposeAsync()` | when the instance goes away | no-op |

All the `Create*` methods run on the UI thread. `IWidgetPlugin` is
`IAsyncDisposable`, so clean up timers and subscriptions in `DisposeAsync`.

## What `InitializeAsync` gives you

The `IWidgetContext` you receive (and which `WidgetPluginBase` parks in the
`Context` property) is your line back to the host:

- `WidgetId` and `InstanceId`. The second one is unique per placed copy, which is
  how you tell two instances of the same widget apart.
- `SettingsJson`, this instance's saved settings, in whatever shape you chose.
- `DataDirectory`, a per-plugin folder, already created, for caches and files.
- `RequestPreviewRefresh()`, which you call whenever your data, visibility or
  preferred preview width changes.

## Adding settings

If your widget has anything to configure, implement `IConfigurableWidgetPlugin`:

```csharp
public sealed class WeatherPlugin : WidgetPluginBase, IConfigurableWidgetPlugin
{
    public UIElement? CreateSettingsContent(IWidgetSettingsContext ctx) =>
        new WeatherSettingsView(ctx);
}
```

The moment you implement it, two things happen for free: the flyout grows a gear
button, and WidBar's *Configure* action opens your settings window. Inside, the
`IWidgetSettingsContext` hands you the current `SettingsJson`, a
`SaveSettings(json)` to persist when the user confirms, and
`RequestPreviewRefresh()`.

The JSON schema is entirely yours. WidBar just stores the string per instance and
hands it back on the next `InitializeAsync`. If you want the preview to update
live while the user is still editing, override `OnSettingsDraftChanged`; it fires
on every change, and once more with the original JSON if they cancel, so you
naturally revert.

## Keeping a flyout warm (optional)

The SDK keeps your flyout alive after first use so reopening it is instant. If
that means leaving timers or subscriptions running while it's hidden, implement
`IWidgetFlyoutLifecycle` and pause them:

```csharp
public interface IWidgetFlyoutLifecycle
{
    void OnFlyoutShown();
    void OnFlyoutHidden();
}
```

## Two things that bite people

The first is statics. One process, many instances: anything you put in a `static`
field is shared across every copy of your widget, which is almost never what you
want. Keep state on the instance.

The second is threads. The `Create*` methods and `OnSettingsDraftChanged` come in
on the UI thread, but if your data updates on a background thread (a timer, a
network callback), marshal back to the UI thread before touching XAML, then call
`RequestPreviewRefresh()`.

The enums, for reference:

```csharp
enum WidgetCategory { Utility, System, Media, Productivity, Information, Developer, Entertainment }
enum WidgetFlyoutBackdrop { Transparent, Mica, Acrylic }
```

Next up: what each surface can actually contain, in
[[Preview, flyout & settings|Preview-Flyout-Settings]].
