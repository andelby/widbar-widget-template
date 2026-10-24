# Taskbar preview

The preview is what sits on the taskbar all the time. You return a normal
WinUI `UIElement` from `CreatePreviewContent()` and the SDK draws it live in
your spot: bindings, animations, pointer input and standard controls all work.
It is not a picture of your UI, it is your UI.

```csharp
public override int PreviewLogicalWidth => _settings.Compact ? 120 : 188;

public override UIElement? CreatePreviewContent()
{
    _label = new TextBlock
    {
        Text = "Sunny",
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
    };
    return new Grid { Children = { _label } };
}
```

## Size

| | |
| --- | --- |
| Width | `PreviewLogicalWidth`, in logical pixels (96 DPI). Default 188. Read after `InitializeAsync` and after every settings change |
| Height | The height of the taskbar: about 48 logical pixels normally, about 26 when the user picks small taskbar buttons |

Test both taskbar heights. A `MinHeight` on the root of the preview fights the
small taskbar; fixed `LineHeight` values on text work much better. When the
width changes (a compact mode in your settings, for example), call
`Context.RequestPreviewRefresh()` so WidBar makes room.

## Showing and hiding

There are two different "visible" flags, and they answer different questions:

| Flag | Who sets it | Means |
| --- | --- | --- |
| `IWidgetPlugin.IsPreviewVisible` | You | Whether the widget wants to be on the taskbar at all. A media widget can hide while nothing is playing |
| `IWidgetContext.IsPreviewVisible` | WidBar | Whether your preview is on screen right now. It is false for a stacked widget that is not the one showing |

To hide yourself, return `false` from `IsPreviewVisible` and call
`RequestPreviewRefresh()`. The instance stays alive and configurable; flip it
back and refresh to come back.

To save work nobody can see, listen to the context flag:

```csharp
public override async Task InitializeAsync(IWidgetContext context)
{
    await base.InitializeAsync(context);
    context.PreviewVisibilityChanged += OnPreviewVisibilityChanged;
    SetPreviewTimer(context.IsPreviewVisible);
}

private void OnPreviewVisibilityChanged(object? sender, bool isVisible) =>
    SetPreviewTimer(isVisible);
```

Unsubscribe in `DisposeAsync`.

## Smart Stack

Users can put several widgets in one spot, a Smart Stack, which rotates between
them. Only the one in front is visible; the others get
`IWidgetContext.IsPreviewVisible == false` until their turn. When something
happens that the user should not miss (a timer that just finished, a new
message), call `Context.RequestAttention()` and WidBar brings your widget to the
front. Outside a stack the call does nothing, so it is always safe.

## Clicks and hover

| Where the user clicks | What happens |
| --- | --- |
| On one of your controls (`Button`, `ToggleButton`, a slider) | The control gets the click, the flyout does not open |
| Anywhere else in the preview | The flyout opens |

While the pointer is over the preview, WidBar lights all of it, as the taskbar
lights one of its own buttons. Over one of your controls the light goes out,
because the control shows its own hover. So a preview with an area that opens
the flyout and a few buttons, like a media player, needs nothing.

If your preview is only a row of separate buttons, with no flyout area between
them, return `false` from `HighlightPreviewOnHover`, so only the button under
the pointer lights up. It is read once, when the preview is created.

### Buttons that look like the taskbar's

`TaskbarButtonResources` gives your buttons the same hover and pressed plate as
the taskbar's own buttons, in both themes. Merge it into `App.xaml` next to
`XamlControlsResources` (the templates already do):

```xml
<hosting:WidgetHostApplication ...
    xmlns:sdk="using:WidBar.SDK">
    <hosting:WidgetHostApplication.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
                <sdk:TaskbarButtonResources />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </hosting:WidgetHostApplication.Resources>
</hosting:WidgetHostApplication>
```

Then use its brushes in your button's template. The plate needs a border of 1:

```xml
<ControlTemplate TargetType="Button">
    <Grid x:Name="Plate"
          Background="Transparent"
          BorderBrush="Transparent"
          BorderThickness="1"
          CornerRadius="4">
        <VisualStateManager.VisualStateGroups>
            <VisualStateGroup x:Name="CommonStates">
                <VisualState x:Name="Normal" />
                <VisualState x:Name="PointerOver">
                    <VisualState.Setters>
                        <Setter Target="Plate.Background" Value="{ThemeResource WidBarTaskbarButtonBackgroundPointerOver}" />
                        <Setter Target="Plate.BorderBrush" Value="{ThemeResource WidBarTaskbarButtonBorderBrushPointerOver}" />
                    </VisualState.Setters>
                </VisualState>
                <VisualState x:Name="Pressed">
                    <VisualState.Setters>
                        <Setter Target="Plate.Background" Value="{ThemeResource WidBarTaskbarButtonBackgroundPressed}" />
                        <Setter Target="Plate.BorderBrush" Value="{ThemeResource WidBarTaskbarButtonBorderBrushPressed}" />
                    </VisualState.Setters>
                </VisualState>
            </VisualStateGroup>
        </VisualStateManager.VisualStateGroups>
        <ContentPresenter Content="{TemplateBinding Content}"
                          ContentTemplate="{TemplateBinding ContentTemplate}"
                          HorizontalAlignment="Center"
                          VerticalAlignment="Center" />
    </Grid>
</ControlTemplate>
```

UI built in code gets the same brushes from two methods. Pass the element's
`ActualTheme`, which is the taskbar's theme:

```csharp
var plate = new Border
{
    Height = TaskbarButtonResources.PlateHeight,
    CornerRadius = new CornerRadius(TaskbarButtonResources.PlateCornerRadius),
    BorderThickness = new Thickness(TaskbarButtonResources.PlateBorderThickness),
};

plate.PointerEntered += (_, _) =>
{
    plate.Background = TaskbarButtonResources.CreateBackgroundBrush(plate.ActualTheme);
    plate.BorderBrush = TaskbarButtonResources.CreateBorderBrush(plate.ActualTheme);
};
plate.PointerExited += (_, _) =>
{
    plate.Background = null;
    plate.BorderBrush = null;
};
```

For the pressed look pass `pressed: true`. The SDK merges the dictionary at
start if `App.xaml` does not, but code that runs before that (a design-time
window, for example) only finds the keys in `App.xaml`.

## Theme

The preview follows the taskbar's theme, which can differ from the apps' theme.
Keep the root of the preview transparent, don't paint the taskbar background
yourself, and use `ThemeResource` brushes:

```xml
<Grid Background="Transparent">
    <TextBlock Foreground="{ThemeResource TextFillColorPrimaryBrush}" Text="Sunny" />
</Grid>
```

Standard controls update when the theme changes. If you cache colors in code,
listen to `ActualThemeChanged` on the root and rebuild only those brushes.

## Drag and drop

Dropping files on the preview uses normal WinUI drag and drop:

```csharp
root.AllowDrop = true;
root.DragOver += (_, e) =>
{
    e.AcceptedOperation = DataPackageOperation.Copy;
    Context?.RequestOpenFlyout(); // optional: open the flyout so the drag can go on there
};
root.Drop += async (_, e) =>
{
    if (e.DataView.Contains(StandardDataFormats.StorageItems))
    {
        var items = await e.DataView.GetStorageItemsAsync();
        // use the files
    }
};
```

`RequestOpenFlyout()` only opens, it never closes an open flyout, so calling it
on every `DragOver` is fine.

## A layout trap

If you redraw shapes in response to your own `SizeChanged` (a sparkline that
stretches with the preview, say), draw them in a `Canvas` with fixed
coordinates and clip the canvas. Resizing elements that take part in layout from
inside `SizeChanged` can end in a `LayoutCycleException`, and an unclipped line
happily paints over the text next to it.
