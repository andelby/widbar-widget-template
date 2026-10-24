# Changelog

Changes to WidBar.SDK that matter to widget developers. The package is on
[NuGet](https://www.nuget.org/packages/WidBar.SDK).

## 2.2.2

- New `TaskbarButtonResources`: the hover and pressed plate of the Windows 11
  taskbar buttons, as resources to merge in `App.xaml` or as brushes for UI
  built in code (`CreateBackgroundBrush`, `CreateBorderBrush`).
- The whole-preview hover matches the taskbar in both themes and dims while
  pressed.

## 2.2.1

- A flyout always opens at the size WidBar gives it, also on a display at
  another scale.
- A flyout can no longer open twice.
- A flyout that ends up behind other windows comes back on top.
- Clicks and flyouts respond faster while your widget is busy.

## 2.2.0

- The flyout slides out of the taskbar like the Windows 11 quick settings, and
  stays smooth while your widget is busy.
- Large flyouts no longer pause or flash on their first open, and settings
  windows no longer flash black while resized.

## 2.1.2

- A widget no longer crashes when WidBar closes.
- The preview fills the whole height of the taskbar.
- New `HighlightPreviewOnHover`, for previews made of separate buttons.

## 2.1.1

- Switching Windows between light and dark no longer crashes widgets.
- Built against Windows App SDK 2.5.1.

## 2.1.0

- Every flyout sits on the material the user picks in WidBar (acrylic, dense
  acrylic or Mica). `FlyoutBackdrop` and `WidgetFlyoutBackdrop` are obsolete
  and ignored; existing overrides still compile.
- A stacked widget hidden while the pointer was over it no longer keeps its
  hover.
