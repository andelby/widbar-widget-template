# Getting started

## What you need

You'll need Windows 11 with Developer Mode turned on (Settings, System, For
developers) so you can run unsigned local builds. For the toolchain, either
Visual Studio 2022+ with the *Windows application development* workload, or the
.NET 8 SDK plus the Windows App SDK if you prefer the command line. And you need
WidBar itself installed from the Store, since it's the host that runs whatever
you build. Without it there's nothing to see.

## Install the template

Clone or download the
[template repo](https://github.com/andelby/widbar-widget-template), then install
it from that folder:

```powershell
dotnet new install .
```

You only do this once. After that the `widbar-widget` template is available to
`dotnet new`.

## Scaffold a widget

```powershell
dotnet new widbar-widget -n Contoso.Weather --PluginId com.contoso.weather --DisplayName "Contoso Weather"
```

`--PluginId` is your widget's permanent identity: reverse-DNS, unique, and you
don't want to change it after release because it's how WidBar tracks installs.
`--DisplayName` is the friendly name people see.

You end up with two projects:

```text
Contoso.Weather.ExtensionApp/   the widget itself: plugin class, preview, flyout, settings
Contoso.Weather (Package)/       the MSIX packaging project you actually publish
```

The ExtensionApp only references `WidBar.SDK`. Because it runs in its own
process, you can pull in any other package or native dependency you want without
worrying about clashing with WidBar or other people's widgets.

## Build the right project

Build the packaging project, not the ExtensionApp directly. The packaging project
is what produces something Windows can install. In Visual Studio, set
`Contoso.Weather (Package)` as the startup project and build. From the command
line:

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


## See it on the taskbar

Open WidBar, go to the Layout page, and your widget will be sitting in the
catalog. Drag it onto a free space in the taskbar preview and it shows up on the
real taskbar straight away. WidBar watches the AppExtension catalog, so when you
rebuild and redeploy it picks up the new version on its own. No need to restart
WidBar.

If WidBar is already open while you redeploy, refresh the widget catalog or reopen
the Layout page to see the updated package. If deployment reports a file-in-use
error, stop that specific widget instance or end its ExtensionApp process, then
deploy again.

From here, [[the plugin contract|Plugin-Contract]] walks through the class you
just got handed, and [[Preview, flyout & settings|Preview-Flyout-Settings]]
covers what goes in each surface.
