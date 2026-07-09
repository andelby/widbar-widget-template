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

## Where catalog metadata lives

The description and category people see in the WidBar catalog are **not** on the
plugin class. They come from your `plugin.json`, generated from the `WidBarPlugin*`
properties in your `.csproj` (see [[Packaging & publishing|Packaging-and-Publishing]]).
That's the single source: `WidBarPluginName`, `WidBarPluginDescription`,
`WidBarPluginCategory`, `WidBarPluginVersion`. The plugin class carries only what
the running widget needs.

## The members you'll touch

`Id` and `Name` are the runtime identity (the id is the handshake key; the name
titles the standalone-launch window). Everything else is layout or lifecycle.

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

- `InstanceId`, unique per placed copy, which is how you tell two instances of
  the same widget apart. (Your own `Id` constant identifies the widget itself.)
- `SettingsJson`, this instance's saved settings, in whatever shape you chose.
- `DataDirectory`, a per-plugin folder, already created, for caches and files.
- `RequestPreviewRefresh()`, which you call whenever your data, visibility or
  preferred preview width changes.
- `RequestOpenFlyout()`, which asks the host to open your flyout. Its main use
  is spring-loading: call it from your preview's `DragOver` handler so a
  file drag over the taskbar opens the flyout and the drag can continue onto
  it. It only ever opens (never closes), so calling it repeatedly is safe.
- `RequestAttention()`, which asks the host to bring your widget to the front.
  When it is a member of a smart stack (several widgets sharing one taskbar
  slot), the host makes it the visible member so a timely event is not missed,
  e.g. a finished timer or an incoming notification. No-op when not stacked.

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

## The flyout lives longer than you think

This one earns its own section because it bites even people who know better.
`CreateFlyoutContent()` is called ONCE and the returned element is reused for
every open: closing the flyout hides the window, it does not discard your UI.
Two consequences:

- Your element's `Unloaded` event fires at every close. If you tear down event
  subscriptions or data bindings there, the second open shows a UI that never
  updates again. Subscriptions that should live as long as the flyout content
  belong to the content's lifetime, not to a single open/close cycle.
- Timers and polling SHOULD pause while hidden, though. That's what
  `IWidgetFlyoutLifecycle` is for: implement it on your plugin and the SDK
  tells you exactly when the flyout window shows and hides.

```csharp
public interface IWidgetFlyoutLifecycle
{
    void OnFlyoutShown();    // resume timers, do one fresh render
    void OnFlyoutHidden();   // pause timers; keep subscriptions alive
}
```

The rule of thumb: pause *work* on hide, never disconnect *state*.

## Two things that bite people

The first is statics. One process, many instances: anything you put in a `static`
field is shared across every copy of your widget, which is almost never what you
want. Keep state on the instance.

The second is threads. The `Create*` methods and `OnSettingsDraftChanged` come in
on the UI thread, but if your data updates on a background thread (a timer, a
network callback), marshal back to the UI thread before touching XAML, then call
`RequestPreviewRefresh()`.

For reference: `FlyoutBackdrop` is the `WidgetFlyoutBackdrop` enum
(`Transparent`, `Mica`, `Acrylic`). And the category you set in the csproj
(`WidBarPluginCategory`) is one of: `Utility`, `System`, `Media`,
`Productivity`, `Information`, `Developer`, `Entertainment`.

Next up: what each surface can actually contain, in
[[Preview, flyout & settings|Preview-Flyout-Settings]].
