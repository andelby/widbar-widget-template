# Quick start

From nothing to a widget on your taskbar. You need the tools listed on the
[home page](Home.md#what-you-need) and WidBar installed.

## 1. Install the templates

Clone or download [the template repository](https://github.com/andelby/widbar-widget-template),
then install the templates from its folder:

```powershell
dotnet new install .
```

You get two templates:

| Template | Use it when |
| --- | --- |
| `widbar-widget` | The widget is a product of its own, with its own Store listing |
| `widbar-companion` | You already ship a packaged WinUI app and the widget should come with it ([Companion widgets](Companion-Widgets.md)) |

## 2. Create a widget

```powershell
dotnet new widbar-widget -n Contoso.Weather `
    --PluginId com.contoso.weather `
    --DisplayName "Contoso Weather"
```

`--PluginId` is the permanent identity of your widget, in reverse DNS form.
WidBar uses it to remember where the widget is placed and its settings, so
don't change it after you release. `--DisplayName` is the name people see.

You end up with two projects:

```text
Contoso.Weather.ExtensionApp/   the widget process: plugin class, preview, flyout, settings
Contoso.Weather (Package)/      the MSIX packaging project, the thing you install and publish
```

The sample is a clock: a small one on the taskbar, a big one in the flyout and
a 12 or 24 hour switch in its settings. Replace it with your own UI.

## 3. Build and deploy

Build the **packaging project**, not the app project. Only the package can be
installed, and installing it is what tells Windows about your AppExtension.

In Visual Studio: set the packaging project as the startup project, choose
`Debug | x64`, then **Build > Deploy**.

From the command line, restore and build as two separate steps, then register
the layout:

```powershell
$project = "Contoso.Weather (Package)\Contoso.Weather (Package).wapproj"
msbuild $project -t:Restore -p:Configuration=Debug -p:Platform=x64
msbuild $project -t:Build -p:Configuration=Debug -p:Platform=x64 -p:AppxBundle=Never
Add-AppxPackage -Register "Contoso.Weather (Package)\bin\x64\Debug\AppxManifest.xml"
```

## 4. Put it on the taskbar

Open WidBar and go to the **Layout** page. Your widget is in the catalog: drag
it onto a free spot of the taskbar picture and it appears on the real taskbar.
Click it to open the flyout.

## 5. Change something

Edit `MainPlugin.cs`, build and deploy again. WidBar notices the new build on
its own; if the Layout page is already open, use **Refresh**. If deploying
fails because files are in use, remove the widget from the taskbar (or end its
process) and deploy again.

## Next

- [Lifecycle](Lifecycle.md): when your code runs, copies of the widget and threads.
- [Taskbar preview](Taskbar-Preview.md), [Flyout](Flyout.md) and [Settings](Settings.md): building each surface.
- [Debugging](Debugging.md): logs and attaching a debugger.
