# Preview, flyout & settings

A widget can offer three surfaces. Only the preview is really essential, since a
widget with no preview has nothing to put on the taskbar, but most widgets use at
least the preview and the flyout.

## The preview

`CreatePreviewContent()` returns whatever you want sitting in that little slot on
the taskbar. Return a normal WinUI `UIElement`: text, icons, images, paths,
panels, controls and lightweight animations are all valid. WidBar owns placement,
hover, click-to-open and the taskbar slot itself; your job is to provide a compact
piece of UI that reads quickly.

Design for the size you ask for in `PreviewLogicalWidth`, and remember it's
re-read after settings change, so a "compact mode" toggle can genuinely shrink
the slot:

```csharp
public override int PreviewLogicalWidth => _settings.Compact ? 120 : 188;
public override bool IsPreviewVisible    => !_settings.HideWhenIdle || _isActive;
public override UIElement? CreatePreviewContent() => _preview = new MyPreview(_settings);
```

WidBar owns the slot itself: placement, sizing, hover, and the click that opens
the flyout. You just draw. When your data changes, call
`Context.RequestPreviewRefresh()`. Call it after data changes, after settings
change the preferred width, or after `IsPreviewVisible` changes. If there's
nothing worth showing, set `IsPreviewVisible` to false rather than rendering an
empty box. The instance stays alive and configurable; it just disappears from the
taskbar until you flip it back and request a refresh.

### The preview is live and interactive

The preview is your actual WinUI element rendered on the taskbar, not a
screenshot of it. That means:

- **Controls work.** Put a `Button` in the preview and its `Click` fires
  normally; the click is consumed by the button and does not open the flyout.
  Clicking anywhere that is not an interactive control opens the flyout as
  usual. A tiny play/pause button next to a timer, straight on the taskbar, is
  a couple of lines.
- **Animations run** at full frame rate. Keep them subtle; it's the taskbar.
- **File drag & drop is native.** Set `AllowDrop` on your preview and handle
  `DragOver`/`Drop` like in any window. Pair it with
  `Context.RequestOpenFlyout()` in `DragOver` to spring-load the flyout so the
  user can continue the drag onto a bigger drop target.

One layout caution: if you redraw shapes in response to your own `SizeChanged`
(a sparkline that stretches, say), draw into a `Canvas` with fixed coordinates
rather than resizing layout-participating shapes, and clip the canvas. Redrawing
layout-affecting elements from `SizeChanged` can spiral into a `LayoutCycleException`,
and an unclipped line happily paints over the text next to it.

### Light and dark themes

WidBar applies the current Windows shell theme to the preview, flyout and
settings host. There is no theme API to call from the plugin.

Keep the preview root transparent and use WinUI theme resources instead of
fixed foreground or background colors:

```xml
<Grid Background="Transparent">
    <Border
        Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
        BorderThickness="1"
        CornerRadius="6">
        <TextBlock
            Foreground="{ThemeResource TextFillColorPrimaryBrush}"
            Text="Weather" />
    </Border>
</Grid>
```

Standard controls update when Windows changes theme. If a custom renderer
caches colors, listen to `ActualThemeChanged` on its root element and rebuild
only those cached brushes. Do not paint the taskbar background yourself.

### Smart stack visibility

Only one member of a smart stack is visible at a time. Use
`IWidgetContext.IsPreviewVisible` and `PreviewVisibilityChanged` to stop
animation, polling or rendering that only serves the preview while another
member is on top.

Call `RequestAttention()` when an important event should bring the widget to
the top, such as a completed timer. The call is safe outside a stack and does
nothing there.

## The flyout

`CreateFlyoutContent()` is the popup that opens when someone clicks the preview.
Return `null` if your widget doesn't need one. This is where richer controls,
larger layouts, scrolling, input and detailed state usually belong.

Size it with `FlyoutWidth` and `FlyoutHeight`, and pick a backdrop. The backdrop
is just the window material behind your content; you still bring your own padding
and colours.

```csharp
public override int FlyoutWidth  => 360;
public override int FlyoutHeight => 420;
public override WidgetFlyoutBackdrop FlyoutBackdrop => WidgetFlyoutBackdrop.Acrylic;
public override UIElement? CreateFlyoutContent() => new MyFlyoutView(_settings);
```

The SDK keeps the flyout warm after the first open so it reappears instantly. If
your flyout does ongoing work, implement `IWidgetFlyoutLifecycle` (see
[[the plugin contract|Plugin-Contract]]) and pause it in `OnFlyoutHidden`.

When a file picker or another modal window must open from the flyout, wrap it in
`WidgetFlyout.EnterModalScope()`. This prevents the flyout from closing while
focus is temporarily owned by the modal window.

## Settings

Implement `IConfigurableWidgetPlugin.CreateSettingsContent` and WidBar hosts your
UI in a tidy Mica window with Save and Cancel buttons (it localises those labels
for you). The same window opens from the flyout's gear button and from
*Configure* in WidBar.

```csharp
public UIElement? CreateSettingsContent(IWidgetSettingsContext ctx)
{
    var view = new MySettingsView(MySettings.FromJson(ctx.SettingsJson));
    view.Changed += s => ctx.SaveSettings(s.ToJson());   // persisted when the user hits Save
    return view;
}
```

The schema is yours; WidBar only ever sees a JSON string and hands it back on the
next init. The nice part is live preview: `OnSettingsDraftChanged(json)` fires on
every edit, so the taskbar updates as the user drags a slider. It's also called
once with the original JSON if they cancel, which means re-applying it naturally
undoes the draft:

```csharp
public override void OnSettingsDraftChanged(string json)
{
    _settings = MySettings.FromJson(json);
    MyPreview.Apply(_preview, _settings);
}
```

## If you use a control library

Some XAML control libraries need their resource dictionary merged in. Do it in
the widget application definition the usual way. If the library ships a loose
dictionary, call `MergeResourceDictionaryFromFile(relativePath)` from your
`App`.

Ship third-party managed or native DLLs as normal package payload next to the
widget executable. They belong to your package and are loaded by your widget
process, not by WidBar or another widget.
