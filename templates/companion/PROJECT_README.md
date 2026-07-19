# WidBarCompanion1

A WidBar companion widget project, created from the `widbar-companion` template.

Use this when you already have a packaged WinUI app and want the same MSIX
package to expose a WidBar taskbar widget. This project is only the tiny widget
exe. Your existing app stays separate, and your existing package project hosts
both applications.

## What's in here

```text
WidBarCompanion1.Widget/   widget exe: plugin class, preview, flyout, settings
```

There is no packaging project in this template. Add this project to your
existing solution and wire it into your existing `.wapproj`.

## Add it to your app package

1. Add `WidBarCompanion1.Widget.csproj` to your solution.
2. In your `.wapproj`, add a project reference to this widget project:

   ```xml
   <ProjectReference Include="..\WidBarCompanion1.Widget\WidBarCompanion1.Widget.csproj">
     <SkipGetTargetFrameworkProperties>True</SkipGetTargetFrameworkProperties>
     <PublishProfile>Properties\PublishProfiles\win-$(Platform).pubxml</PublishProfile>
   </ProjectReference>
   ```

3. Link the generated WidBar manifest into the package public folder:

   ```xml
   <Content Include="..\WidBarCompanion1.Widget\obj\widbar\plugin.json">
     <Link>Public\plugin.json</Link>
   </Content>
   ```

4. Add a second `<Application>` entry to your package manifest, pointing to the
   widget exe, and declare the `com.widbar.widget` AppExtension there.

See the companion guide for the full manifest block:
https://github.com/andelby/widbar-widget-template/wiki/Companion-Widgets

## Customize

- Identity and catalog entry: edit the `WidBarPlugin*` properties in
  `WidBarCompanion1.Widget.csproj`.
- Code: edit `MainPlugin.cs`. Return WinUI `UIElement`s for the taskbar
  preview, flyout and optional settings page.
- Shared app data: because this widget ships in your app package, it shares the
  same package identity, `LocalState` and `LocalSettings` as the main app.
- Dependencies: add any NuGet packages the widget needs to this project.
- Theme: use WinUI `ThemeResource` values and keep the preview root
  transparent. WidBar applies the current Windows theme automatically.
- Smart stacks: use `RequestAttention()` for important events and
  `PreviewVisibilityChanged` to pause preview-only updates while hidden.

## Build and run

Build and deploy your existing packaging project. WidBar discovers the companion
widget from the installed AppExtension and shows it in the catalog. Use Refresh
on the Layout page after a redeploy if needed.

If deployment reports locked files, close the running widget process for your
package and deploy again.
