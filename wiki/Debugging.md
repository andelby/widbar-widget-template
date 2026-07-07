# Debugging & diagnostics

## The inner loop

Build the packaging project, deploy it (Visual Studio's **Build > Deploy**, or
`Add-AppxPackage -Register ...\AppxManifest.xml` with Developer Mode on), then
open WidBar and drag your widget onto the taskbar. WidBar watches the
AppExtension catalog, so once it's deployed the first time, later redeploys are
picked up without restarting WidBar.

For normal development you should not need to restart WidBar manually. Build and
deploy the package, then use WidBar's refresh/discovery flow or reopen the Layout
page if the catalog is already visible. If Visual Studio or `Add-AppxPackage`
reports a file-in-use error, stop that specific widget instance from WidBar or
end the widget process as a troubleshooting step.

## Seeing your logs

Anything you write with `Debug.WriteLine` or `Trace.WriteLine`, plus any
exception that escapes one of your SDK callbacks, is forwarded to WidBar's
developer console. To see it, turn on Developer mode in WidBar's settings, then
open the Console page. Your output shows up tagged with your plugin id, like
`[Plugin:com.contoso.weather]`. The console deliberately shows only widget
output; WidBar host diagnostics stay out of your way and go to the host log file.

```csharp
System.Diagnostics.Trace.WriteLine("[Weather] forecast refreshed");
```

If something's going wrong before your code even runs, like a bad handshake or a
startup crash, check the SDK's own log at
`%LOCALAPPDATA%\Packages\<your-package-family>\LocalCache\Local\Temp\widbar-sdk.*.log`.
WidBar's host log lives at `%LOCALAPPDATA%\...\Logs\widbar.log`.

## Attaching a debugger

Your widget runs in a process WidBar launches, not one Visual Studio started, so
hitting F5 on the packaging project won't land your breakpoints once WidBar owns
the process. Use **Debug > Attach to Process** and pick your widget exe.
While you're developing, a `System.Diagnostics.Debugger.Launch()` near the top of
`InitializeAsync` is a quick way to catch it early.

## Running the .exe on its own

If your widget exe gets launched without WidBar, say you double-click the exe,
it doesn't try to render a phantom taskbar preview. Instead the SDK pops a small
window explaining that this is a WidBar widget and needs WidBar to run. That's
expected behaviour, not a bug.

## When it crashes

WidBar restarts a crashed widget with backoff. If it crashes three times in a row
it gets disabled, and stays disabled until the user re-enables it (there's a
*Widget crash protection* reset in WidBar's settings, and a Re-enable action on
the widget). The practical takeaway: don't let exceptions escape
`InitializeAsync` or the `Create*` methods. Catch and degrade gracefully instead.

## The usual suspects

If your widget never appears in the catalog, it's almost always that the package
was built but not registered (deploy it), or that `Name="com.widbar.widget"` got
changed in the manifest, or that `plugin.json` isn't being linked into the
`Public` folder.

If the preview renders blank, first check the WidBar developer console for an
exception from `CreatePreviewContent()`. Then verify your resources are included
in the package payload and that any third-party XAML resource dictionaries are
merged before the preview is created.

And if the settings gear never shows up, you haven't implemented
`IConfigurableWidgetPlugin`.

There's more in the [[FAQ]].
