# Changelog

Changes to WidBar.SDK that matter to widget developers. The package is on
[NuGet](https://www.nuget.org/packages/WidBar.SDK).

## 2.3.0

- The widget board, WidBar's full screen mosaic of widgets. A widget's tile
  shows its flyout by default, with nothing to do in the widget. See
  [Widget board](Widget-Board.md).
- New `CreateTileContent` and `TileHeight`, to give the board content of its
  own. `IWidgetContext.IsBoardTile` tells a tile from a taskbar copy.
- Board elements: list them in a `BoardElements.json` next to the project
  file, and the board offers each one as a tile of its own.
  `IWidgetContext.BoardElement` says which one a tile is.
- New `IWidgetTileLifecycle`, to resume and pause live work as a tile comes
  and goes.
- New `IWidgetTileSettings`: the gear on a tile can turn the tile into its own
  settings, in place.
- New `IWidgetContext.SharedDataDirectory`, for the data every copy of a
  widget shares. `DataDirectory` belongs to one copy, and a tile's is deleted
  when the tile is taken off the board.
- Names and descriptions in `plugin.json` and `BoardElements.json` can contain
  semicolons.
- The package carries the API's documentation, for IntelliSense.
- Built against Windows App SDK 2.6.4.

## 2.2.3

- A widget added to a Smart Stack takes the stack's width from the start.
- A widget removed while it is still starting no longer leaves its preview
  behind on the taskbar.

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
