# FAQ

**Do I need WidBar installed to develop a widget?**
Yes. WidBar starts your process and gives it a spot on the taskbar, so without
it there is nothing to run. Install it from the Microsoft Store.

**How big can the preview be?**
You ask for a width with `PreviewLogicalWidth` and WidBar fits it into a free
spot. The height is the taskbar's, about 48 logical pixels or about 26 with
small taskbar buttons. Think glance, not dashboard; the flyout is for detail.

**Can users click things in the preview?**
Yes. Your controls get their clicks, and a click anywhere else opens the
flyout. See [Taskbar preview](Taskbar-Preview.md#clicks-and-hover).

**The user placed my widget twice. How do I keep the copies apart?**
`CreatePlugin()` runs once for each copy, and each one has its own settings and
`InstanceId`. Keep state in instance fields, never in `static` ones.

**Where do I keep files?**
In `IWidgetContext.DataDirectory`, which already exists. It belongs to the
widget, so all copies share it: use the `InstanceId` in file names for data
that is per copy.

**How do I support light and dark?**
Use `ThemeResource` brushes and keep the preview root transparent. The SDK
applies the taskbar's theme for you. See [Taskbar preview](Taskbar-Preview.md#theme).

**Can I choose the flyout background?**
No, the user picks the material in WidBar for every widget. Leave your flyout's
root transparent. See [Flyout](Flyout.md#the-background).

**Can I use NuGet packages or native libraries?**
Yes. Your widget is its own process, so nothing you load affects WidBar or
other widgets. Everything ships as normal package payload.

**Can I use a WinUI control library?**
Yes. Merge its resources in `App.xaml` the usual way, or call
`MergeResourceDictionaryFromFile` for a loose dictionary.

**My flyout stops updating after I close and reopen it.**
The flyout content is created once and reused, and `Unloaded` fires at every
close. Don't drop subscriptions there; pause timers in `OnFlyoutHidden`. See
[Flyout](Flyout.md#it-lives-longer-than-you-think).

**I already ship an app. Do I need a separate widget package?**
No. Add a small widget exe to your app's package. See [Companion widgets](Companion-Widgets.md).

**How do I bring my widget to the front of a Smart Stack?**
Call `Context.RequestAttention()`. It does nothing outside a stack.

**Which architectures should I build?**
x64 and ARM64.

**How do I ship an update?**
Raise the version in `Package.appxmanifest` (fourth number 0) and
`WidBarPluginVersion`, build the `.msixupload` and submit it in Partner Center.
See [Packaging and publishing](Packaging-and-Publishing.md).

**How do I get into WidBar's in-app showcase?**
Open a pull request against [andelby/widbar](https://github.com/andelby/widbar)
that adds your widget to `catalog.json`. See [Packaging and publishing](Packaging-and-Publishing.md#6-getting-into-widbars-showcase).

**Where do I report a problem with the SDK?**
In the [WidBar issues](https://github.com/andelby/widbar/issues). Say which SDK
version you use.
