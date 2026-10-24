# Debugging

## The inner loop

1. Build and deploy the packaging project (Visual Studio **Build > Deploy**, or
   `Add-AppxPackage -Register` with Developer Mode on, see [Quick start](Quick-Start.md#3-build-and-deploy)).
2. WidBar notices the new build on its own. If the Layout page is open, press
   **Refresh**.
3. If deploying fails because files are in use, remove the widget from the
   taskbar or end its process, then deploy again.

There is no need to restart WidBar between builds.

## Logs

| What | Where |
| --- | --- |
| Your `Debug.WriteLine` and `Trace.WriteLine` output, and exceptions from your SDK callbacks | WidBar's developer console: turn on Developer mode in WidBar's settings, then open the Console page. Lines are tagged `[Plugin:<your id>]` |
| Your widget's log file, one per copy | `%LOCALAPPDATA%\Packages\<your package family name>\LocalState\Logs\widbar-sdk.*.log` |
| Problems very early at startup, before your widget is shown | `widbar-sdk.log` in your package's temp folder |

```csharp
System.Diagnostics.Trace.WriteLine("[Weather] forecast refreshed");
```

## Attaching a debugger

WidBar starts your process, not Visual Studio, so F5 on the packaging project
does not catch it. Use **Debug > Attach to Process** and pick your widget's
exe. To stop at the very start, put this at the top of `InitializeAsync` while
you develop:

```csharp
System.Diagnostics.Debugger.Launch();
```

## Crashes

WidBar restarts a widget that crashed, waiting a little longer each time (up to
30 seconds). After three crashes in a row the widget is turned off until the
user turns it back on in WidBar. So don't let exceptions escape: catch them in
`InitializeAsync`, in your timers and in network callbacks, and show something
sensible instead.

While developing, ending the process by hand counts as a crash too. Three in a
row and the widget turns off; turn it back on from WidBar.

## Symptoms

| Symptom | Likely cause | Fix |
| --- | --- | --- |
| The widget is not in WidBar's catalog | The package is built but not deployed, or `plugin.json` is not in `Public`, or the extension name changed | Deploy, check the `Content` link and `Name="com.widbar.widget"` |
| The old version keeps showing | Only built, not deployed | Deploy, then Refresh on the Layout page |
| The preview is empty | An exception in `CreatePreviewContent()`, or a resource missing from the package | Check the developer console; make sure control library dictionaries are merged in `App.xaml` |
| The preview looks wrong in one theme | Fixed colors | Use `ThemeResource` brushes and a transparent root |
| The preview is cut off on a small taskbar | A `MinHeight` or tall fixed sizes | Design for about 26 logical pixels of height too |
| A `LayoutCycleException` | Shapes resized from inside `SizeChanged` | Draw them in a clipped `Canvas` with fixed coordinates |
| The flyout stops updating after the first close | Subscriptions dropped in `Unloaded` | Keep them; pause timers in `OnFlyoutHidden` instead |
| A file picker closes the flyout | Focus moved to the picker | Wrap it in `WidgetFlyout.EnterModalScope()` |
| The settings gear is missing | `IConfigurableWidgetPlugin` not implemented | Implement it |
| Two copies of the widget share data | State kept in `static` fields | Keep state in instance fields |
| Running the exe shows a small information window | It was started without WidBar | Expected: widgets run inside WidBar |
