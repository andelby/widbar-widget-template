# API reference

Everything in WidBar.SDK 2.2.2 that a widget uses. Types are in the `WidBar.SDK`
namespace, except `WidgetHostApplication`, which is in `WidBar.SDK.Hosting`.

| Type | Kind | In one line |
| --- | --- | --- |
| [`WidgetHostApplication`](#widgethostapplication) | abstract class | Your `App` class derives from it |
| [`IWidgetPlugin`](#iwidgetplugin) | interface | The contract every widget implements |
| [`WidgetPluginBase`](#widgetpluginbase) | abstract class | `IWidgetPlugin` with defaults, override what you need |
| [`IWidgetContext`](#iwidgetcontext) | interface | What WidBar gives each instance |
| [`IConfigurableWidgetPlugin`](#iconfigurablewidgetplugin) | interface | Adds a settings page |
| [`IWidgetSettingsContext`](#iwidgetsettingscontext) | interface | What the settings page gets |
| [`IWidgetFlyoutLifecycle`](#iwidgetflyoutlifecycle) | interface | Tells you when the flyout shows and hides |
| [`WidgetFlyout`](#widgetflyout) | static class | Keeps the flyout open during a dialog |
| [`TaskbarButtonResources`](#taskbarbuttonresources) | resource dictionary | The taskbar's hover plate for your buttons |
| [`WidgetCategory`](#widgetcategory) | enum | The catalog categories |
| [`WidgetFlyoutBackdrop`](#obsolete) | enum, obsolete | No longer used |

## WidgetHostApplication

`App.xaml` and `App.xaml.cs` derive from it. It creates your plugin for each copy
of the widget and shows its preview, flyout and settings.

| Member | Description |
| --- | --- |
| `protected abstract IWidgetPlugin CreatePlugin()` | Returns a new plugin object. Called for each copy of the widget; the SDK may also create objects it never initializes, so keep the constructor light |
| `protected override void OnLaunched(LaunchActivatedEventArgs args)` | Starts hosting when WidBar launched the process, or shows a small information window otherwise. Override it only in a companion app that also opens its own window |
| `protected static bool IsWidBarHostLaunch()` | `true` when WidBar started the process as a widget host |
| `protected void MergeResourceDictionaryFromFile(string relativePath)` | Loads a loose XAML resource dictionary from the app folder and merges it into the app resources, for control libraries that ship one |

```csharp
public partial class App : WidgetHostApplication
{
    public App() => InitializeComponent();
    protected override IWidgetPlugin CreatePlugin() => new WeatherPlugin();
}
```

## IWidgetPlugin

Implements `IAsyncDisposable`. Members with a default can be left out.

| Member | Default | Description |
| --- | --- | --- |
| `string Id` | required | Unique id in reverse DNS form, like `com.contoso.weather`. Must match `WidBarPluginId` |
| `string Name` | required | Name of the widget, used by the window shown when the exe runs without WidBar |
| `string Version` | required | Version reported to WidBar, like `1.2.0` |
| `int PreviewLogicalWidth` | `188` | Preview width in logical pixels. Read after `InitializeAsync` and after every settings change |
| `bool IsPreviewVisible` | `true` | `false` hides the preview while the instance stays alive. Call `RequestPreviewRefresh()` after changing it |
| `bool HighlightPreviewOnHover` | `true` | Whether WidBar lights the whole preview under the pointer. Read once |
| `int FlyoutWidth` | required | Flyout width in logical pixels. Read once |
| `int FlyoutHeight` | required | Flyout height in logical pixels. Read once |
| `Task InitializeAsync(IWidgetContext context)` | required | One time setup: read settings, start timers and network calls |
| `UIElement? CreatePreviewContent()` | `null` | The taskbar preview. UI thread |
| `UIElement? CreateFlyoutContent()` | required | The flyout content, or `null` for no flyout. Called once, the content is reused. UI thread |
| `void OnSettingsDraftChanged(string settingsJson)` | does nothing | Called while settings are edited, after Save, and with the original JSON after Cancel. UI thread |
| `ValueTask DisposeAsync()` | required | Stop timers, drop subscriptions. Called when the instance goes away |

## WidgetPluginBase

An abstract `IWidgetPlugin` with defaults for everything except `Id` and `Name`.

| Member | Default |
| --- | --- |
| `protected IWidgetContext? Context` | Set by `InitializeAsync` |
| `Id`, `Name` | abstract |
| `Version` | `"1.0.0"` |
| `PreviewLogicalWidth` | `188` |
| `IsPreviewVisible` | `true` |
| `HighlightPreviewOnHover` | `true` |
| `FlyoutWidth`, `FlyoutHeight` | `390`, `480` |
| `InitializeAsync(context)` | Stores `context` in `Context` |
| `CreatePreviewContent()`, `CreateFlyoutContent()` | `null` |
| `OnSettingsDraftChanged(json)` | does nothing |
| `DisposeAsync()` | does nothing |

If you override `InitializeAsync`, call `base.InitializeAsync(context)` so
`Context` gets set.

## IWidgetContext

Passed to `InitializeAsync`, valid for the whole life of the instance.

| Member | Description |
| --- | --- |
| `string InstanceId` | Identifies this placed copy. Two copies of your widget have different ids |
| `string SettingsJson` | This instance's saved settings. Updated after every save |
| `string DataDirectory` | A folder for your files, already created. It belongs to the widget, so every instance shares it |
| `void RequestPreviewRefresh()` | Asks WidBar to update the preview's spot. Call it after changing `PreviewLogicalWidth` or `IsPreviewVisible`; normal UI updates don't need it |
| `void RequestOpenFlyout()` | Opens your flyout. Never closes an open one, so it is safe to repeat |
| `void RequestAttention()` | Brings the widget to the front of its Smart Stack. Does nothing outside a stack. Any thread |
| `bool IsPreviewVisible` | Whether the preview is on screen right now. False for a stacked widget that is not showing |
| `event EventHandler<bool>? PreviewVisibilityChanged` | Raised on the UI thread when `IsPreviewVisible` changes, with the new value |

## IConfigurableWidgetPlugin

| Member | Description |
| --- | --- |
| `UIElement? CreateSettingsContent(IWidgetSettingsContext context)` | Returns the settings page for one instance. The SDK hosts it in a window with Save and Cancel. Implementing it also adds the gear button to the flyout |

## IWidgetSettingsContext

| Member | Description |
| --- | --- |
| `string InstanceId` | The instance being configured |
| `string SettingsJson` | Its settings when the window opened |
| `void SaveSettings(string settingsJson)` | Updates the pending settings. They are stored when the user presses Save |
| `void RequestPreviewRefresh()` | Asks for a preview refresh, for example after a setting that changes the width |

## IWidgetFlyoutLifecycle

Optional. Implement it on your plugin class.

| Member | Description |
| --- | --- |
| `void OnFlyoutShown()` | The flyout became visible. Resume timers, render once |
| `void OnFlyoutHidden()` | The flyout was hidden. Pause timers, keep subscriptions |

## WidgetFlyout

| Member | Description |
| --- | --- |
| `static IDisposable EnterModalScope()` | Keeps the flyout open while a dialog it owns (file picker, folder picker, message dialog) has the focus. Dispose it when the dialog closes. Scopes nest |

```csharp
using var modal = WidgetFlyout.EnterModalScope();
var file = await picker.PickSingleFileAsync();
```

## TaskbarButtonResources

A `ResourceDictionary` with the hover and pressed plate of the Windows 11
taskbar's own buttons, for both themes. Merge it in `App.xaml` with
`<sdk:TaskbarButtonResources />` (`xmlns:sdk="using:WidBar.SDK"`).

| Resource key | Use |
| --- | --- |
| `WidBarTaskbarButtonBackgroundPointerOver` | Plate background under the pointer |
| `WidBarTaskbarButtonBorderBrushPointerOver` | Plate border under the pointer |
| `WidBarTaskbarButtonBackgroundPressed` | Plate background while pressed |
| `WidBarTaskbarButtonBorderBrushPressed` | Plate border while pressed |

| Member | Value or description |
| --- | --- |
| `const string BackgroundPointerOver`, `BorderBrushPointerOver`, `BackgroundPressed`, `BorderBrushPressed` | The four keys above |
| `const double PlateHeight` | `40` |
| `const double PlateCornerRadius` | `4` |
| `const double PlateBorderThickness` | `1` |
| `static Brush CreateBackgroundBrush(ElementTheme theme, bool pressed = false)` | The plate background for UI built in code. Pass the element's `ActualTheme` |
| `static Brush CreateBorderBrush(ElementTheme theme, bool pressed = false)` | The plate border for UI built in code |

Examples in [Taskbar preview](Taskbar-Preview.md#buttons-that-look-like-the-taskbars).

## WidgetCategory

The categories of WidBar's catalog. You set one in the project with
`WidBarPluginCategory`.

`Utility`, `System`, `Media`, `Productivity`, `Information`, `Developer`, `Entertainment`

## Project properties

The SDK's build targets turn these into `obj\widbar\plugin.json`. Details in
[Packaging and publishing](Packaging-and-Publishing.md).

| Property | Default | Goes into |
| --- | --- | --- |
| `WidBarPluginId` | none; without it no manifest is generated | `id` |
| `WidBarPluginName` | none; required when the id is set | `name` |
| `WidBarPluginDescription` | empty | `description` |
| `WidBarPluginCategory` | `Utility` | `category` |
| `WidBarPluginVersion` | `1.0.0` | `version` |
| `WidBarPluginMinHostVersion` | `1.0.0` | `minHostVersion` |
| `WidBarPluginPreviewWidth` | `188` | `previewWidth` |
| `WidBarPluginConfigurable` | `false` | `configurable` |
| (the assembly name) | | `executable`, as `<AssemblyName>.exe` |

## Obsolete

| Member | Since | Replacement |
| --- | --- | --- |
| `IWidgetPlugin.FlyoutBackdrop`, `WidgetPluginBase.FlyoutBackdrop` | 2.1.0 | None. The user picks the flyout material in WidBar; delete the override |
| `WidgetFlyoutBackdrop` (`Transparent`, `Mica`, `Acrylic`) | 2.1.0 | None |
