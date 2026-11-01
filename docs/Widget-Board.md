# Widget board

The widget board is WidBar's full screen mosaic of widgets. The user opens it
from a button on the taskbar or with a keyboard shortcut, and fills it with
tiles from the catalog. Every tile is a copy of a widget of its own, apart
from the copies on the taskbar.

A widget with a flyout works on the board as it is: its tile shows the flyout's
content. Everything on this page is optional.

## What a tile is

| | On the taskbar | On the board |
| --- | --- | --- |
| `IWidgetContext.IsBoardTile` | `false` | `true` |
| Preview | `CreatePreviewContent()` | none |
| Flyout | `CreateFlyoutContent()` | none |
| Content | the preview and the flyout | `CreateTileContent()`, or the flyout's content when it returns `null` |
| Size | `PreviewLogicalWidth`, `FlyoutWidth`, `FlyoutHeight` | the column's width, `TileHeight` to start with |
| Shown and hidden | `IWidgetFlyoutLifecycle` | `IWidgetTileLifecycle`, or `IWidgetFlyoutLifecycle` without it |

`IsBoardTile` is known from `InitializeAsync` on, so a tile can leave out what
only a preview needs.

## Content of your own

Return a `UIElement` from `CreateTileContent()` to give the board something
other than the flyout, and `TileHeight` for the height the tile starts at, in
logical pixels. The user can make the tile taller or shorter afterwards.

```csharp
public override int TileHeight => 220;

public override UIElement? CreateTileContent() =>
    Context?.IsBoardTile == true ? new WeatherTile(_weather) : null;
```

- Leave the root without a background: the tile sits on acrylic of its own.
- Keep the top right corner free: WidBar draws the tile's gear there.
- Don't play an entrance of your own. The board keeps tiles ready while it is
  closed and brings them in with its own movement; a second entrance on top of
  it shows up as a flash.

## Board elements

A widget can give the board more than its flyout: elements of its own, each a
tile with content made for it, like the weather now, the week ahead or a list
of cities. The board lists them under your widget's name, after the default
element (the flyout's content), which is always there. The user can add any of
them, as many times as they like.

List the elements in a `BoardElements.json` next to the project file, in UTF-8.
The SDK puts them in `plugin.json` when you build.

```json
[
  {
    "id": "now",
    "height": 140,
    "glyph": "\uE706",
    "name": { "en": "Now", "it": "Adesso" },
    "description": { "en": "The weather where you are.", "it": "Il meteo dove sei." }
  }
]
```

| Field | Required | Meaning |
| --- | --- | --- |
| `id` | yes | Your name for the element. `IWidgetContext.BoardElement` gives it back |
| `name` | yes | The name in the board's list, by language |
| `description` | no | One line under the name, by language |
| `height` | no | The tile's starting height, in logical pixels |
| `glyph` | no | A Segoe Fluent Icons character shown beside the name |

Names and descriptions are picked by the user's language: a full tag like
`pt-BR` wins over `pt`, and `en` stands in for a missing one. Use a file with
another name or place by setting `WidBarBoardElementsFile` in the project.

Each tile is an instance of its own, and `IWidgetContext.BoardElement` says
which element it is: the `id` from the file, or `null` for the default.
Answer `CreateTileContent()` and `TileHeight` for that element:

```csharp
public override int TileHeight => Context?.BoardElement switch
{
    "now" => 140,
    "week" => 360,
    _ => 0, // the flyout's height
};

public override UIElement? CreateTileContent() => Context?.BoardElement switch
{
    "now" => new NowTile(_weather),
    "week" => new WeekTile(_weather),
    _ => null, // the flyout's content
};
```

An element does one thing. Show one piece of information well, in the look
of your flyout, rather than a smaller copy of the flyout: the default element
is already there for that.

## Settings on the tile

An element's settings are few (which city, which stock) and belong on the tile
itself. Implement `IWidgetTileSettings`: the gear on the tile then calls
`OnTileSettingsRequested()`, and the tile turns its content into those
settings, the way a phone's widget is edited. Return `true` when the tile
handled it, or `false`, for the default element usually, and WidBar opens your
settings window as for any widget.

```csharp
public bool OnTileSettingsRequested() => _tile?.ToggleEditing() ?? false;
```

Tiles share your widget's settings: a new tile starts with the ones on the
taskbar, and when the user saves them anywhere every copy gets them through
`OnSettingsDraftChanged`. Keep what one tile picks for itself (its city, its
stock) in its own `DataDirectory`.

## Data

| Folder | Whose | Kept |
| --- | --- | --- |
| `IWidgetContext.DataDirectory` | this copy only | while the copy exists. A tile's folder is deleted when the user takes the tile off the board |
| `IWidgetContext.SharedDataDirectory` | every copy of the widget | always |

Put in `SharedDataDirectory` what the user expects to find in every copy: a
watchlist, connected accounts, a running timer. All of a widget's copies run
in one process, so they can share one object in memory as well, created by
the first copy and released by the last:

```csharp
public override async Task InitializeAsync(IWidgetContext context)
{
    await base.InitializeAsync(context);
    _watchlist = await Watchlist.JoinAsync(context.SharedDataDirectory);
}
```

## Live work

The board shows and hides its tiles together. Implement
`IWidgetTileLifecycle` to resume sampling, polling and animations in
`OnTileShown()` and pause them in `OnTileHidden()`, so nothing runs while the
board is away. Both are called on the UI thread.

## Start up quickly

The first time the board opens, every widget on it starts at the same moment.
Compile the release build ahead of time so none of them has to compile its
code then:

```xml
<PropertyGroup>
  <PublishReadyToRun Condition="'$(Configuration)' == 'Release'">true</PublishReadyToRun>
</PropertyGroup>
<ItemGroup>
  <PublishReadyToRunExclude Include="Microsoft.Windows.SDK.NET.dll" />
</ItemGroup>
```
