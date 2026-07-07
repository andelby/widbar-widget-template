# Getting started

## What you need

You'll need Windows 11 with Developer Mode turned on so you can run unsigned
local builds. For the toolchain, use Visual Studio 2022+ with the Windows
application development workload, or the .NET 8 SDK plus Windows App SDK
tooling if you prefer the command line. You also need WidBar installed from the
Microsoft Store, since WidBar is the host that discovers and runs widgets.

## Install the templates

Clone or download the
[template repo](https://github.com/andelby/widbar-widget-template), then install
the templates from that folder:

```powershell
dotnet new install .
```

This installs two templates:

```text
widbar-widget       standalone widget package
widbar-companion    widget exe for an existing packaged app
```

## Option A: scaffold a standalone widget

Use `widbar-widget` when the widget is its own product and will have its own
Store package.

```powershell
dotnet new widbar-widget -n Contoso.Weather `
    --PluginId com.contoso.weather `
    --DisplayName "Contoso Weather"
```

`--PluginId` is your widget's permanent identity: reverse-DNS, unique, and best
kept stable after release because WidBar uses it to track installs and saved
settings. `--DisplayName` is the friendly name people see.

You end up with two projects:

```text
Contoso.Weather.ExtensionApp/   widget process: plugin class, preview, flyout, settings
Contoso.Weather (Package)/      MSIX packaging project you publish
```

Build the packaging project, not the widget process directly. The packaging
project is what produces something Windows can install:

```powershell
msbuild "Contoso.Weather (Package)\Contoso.Weather (Package).wapproj" `
    /p:Configuration=Debug /p:Platform=x64 /restore
```

Deploy the package so Windows registers the AppExtension. In Visual Studio use
**Build > Deploy**. From the command line, register the loose package layout
created under the packaging project's `bin` folder:

```powershell
Add-AppxPackage -Register "<layout>\AppxManifest.xml"
```

## Option B: scaffold a companion widget

Use `widbar-companion` when you already ship a packaged WinUI app and want that
same app package to include a WidBar widget.

```powershell
dotnet new widbar-companion -n Contoso.App `
    --PluginId com.contoso.app.companion `
    --DisplayName "Contoso App"
```

You get one small project:

```text
Contoso.App.Widget/             widget exe to include in your existing package
```

Add the project to your existing solution, reference it from your existing
`.wapproj`, link the generated `obj\widbar\plugin.json` into `Public\plugin.json`,
and add a second hidden `<Application>` entry with the `com.widbar.widget`
AppExtension.

Follow the complete checklist in [[Companion widgets|Companion-Widgets]].

## See it on the taskbar

Open WidBar, go to the Layout page, and your widget appears in the catalog.
Drag it onto a free space in the taskbar preview and it shows up on the real
taskbar. WidBar watches the AppExtension catalog, so redeployed widgets can be
refreshed from WidBar without restarting the app.

If a deploy reports locked files, close the running widget process for that
package and deploy again.

From here, [[the plugin contract|Plugin-Contract]] walks through the class you
just created, and [[Preview, flyout & settings|Preview-Flyout-Settings]] covers
what goes in each surface.
