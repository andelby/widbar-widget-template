# WidBarWidget1

A WidBar taskbar widget, created from the `widbar-widget` template.

New to WidBar widgets? Read the
[developer wiki](https://github.com/andelby/widbar-widget-template/wiki). It
covers the plugin contract, the three UI surfaces, packaging and publishing.

## What's in here

```text
WidBarWidget1.ExtensionApp/   your widget: plugin class, preview, flyout, settings
WidBarWidget1 (Package)/       MSIX packaging project, this is what you publish
```

- The ExtensionApp depends only on the `WidBar.SDK` NuGet package. It runs in its
  own process, so add any dependency you like with no conflicts.
- `MainPlugin.cs` is your entry point: it returns the preview, flyout and
  settings UI. Start there.
- The sample follows the Windows light or dark theme automatically and pauses
  preview-only work when another smart stack member is visible.

## Build and run

1. Build the packaging project (`Debug | x64`):
   - Visual Studio: set `WidBarWidget1 (Package)` as startup, then build, or
   - CLI: `msbuild "WidBarWidget1 (Package)\WidBarWidget1 (Package).wapproj" /p:Configuration=Debug /p:Platform=x64 /restore`
2. Deploy it so Windows registers the AppExtension: Visual Studio
   *Build > Deploy*, or (with Developer Mode on) register the generated loose
   layout with `Add-AppxPackage -Register ...\AppxManifest.xml`.
3. Start WidBar. Your widget shows up in the catalog, so drag it onto the
   taskbar. After a redeploy, use Refresh on the Layout page if the new build is
   not picked up immediately.

## Customize

- Identity and catalog entry: the `WidBarPlugin*` properties in
  `WidBarWidget1.ExtensionApp.csproj` generate `plugin.json` at build time.
  Keep description and category in the project file.
- Code: edit `MainPlugin.cs` or move the UI into your own view classes. Return
  WinUI `UIElement`s for the taskbar preview, flyout and optional settings page.
- Theme: use WinUI `ThemeResource` values and keep the preview root
  transparent. WidBar applies the current Windows theme to every surface.
- Smart stacks: use `RequestAttention()` for important events and
  `PreviewVisibilityChanged` to suspend preview-only updates while hidden.
- Diagnostics: your `Debug.WriteLine` or `Trace.WriteLine` output appears in
  WidBar's developer console as `[Plugin:<id>]`. Enable Developer mode in
  WidBar to see it.

## Before publishing

- Update `Package.appxmanifest` (Identity and Publisher) to your Store
  reservation.
- Replace the placeholder images in `Images\`.
- Submit the packaging project's `.msixupload` to the Microsoft Store, then
  optionally list it in [andelby/winbar-showcase](https://github.com/andelby/winbar-showcase).

See the [wiki](https://github.com/andelby/widbar-widget-template/wiki) for details.
