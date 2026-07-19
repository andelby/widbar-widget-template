# FAQ

**Do I need WidBar installed to develop a widget?**
Yes. WidBar is the host that launches your process and renders the preview, so
without it there's nothing to run against. Install it from the Microsoft Store.

**What can I put in the taskbar preview?**
Return a compact WinUI `UIElement`: text, icons, images, shapes, panels, custom
controls and lightweight animation all fit the model. Keep it glanceable and
taskbar-sized; use the flyout for larger or more detailed experiences. See
[[Preview, flyout & settings|Preview-Flyout-Settings]].

**How big can the preview be?**
You ask for a width via `PreviewLogicalWidth` (logical pixels) and WidBar fits it
into a free taskbar space; height follows the taskbar. Design something that
reads at a glance. It's a taskbar slot, not a dashboard.

**The same widget is placed twice. How do I keep the copies separate?**
One process hosts both, but `CreatePlugin()` runs once per copy. Keep all state
in instance fields (never `static`) and tell the copies apart with
`IWidgetContext.InstanceId`.

**Where should I cache data?**
`IWidgetContext.DataDirectory`, a per-plugin folder that's already created for
you.

**How do settings work?**
Implement `IConfigurableWidgetPlugin`. Your settings are JSON in whatever shape
you like; WidBar stores it per instance and gives it back via
`IWidgetContext.SettingsJson`. Override `OnSettingsDraftChanged` for live preview
while the user edits.

**How do I support light and dark themes?**
Use WinUI `ThemeResource` brushes and leave the preview root transparent.
WidBar applies the current Windows shell theme to preview, flyout and settings
surfaces automatically. Listen to `ActualThemeChanged` only when a custom
renderer caches concrete colors.

**How should my widget behave inside a smart stack?**
Use `IWidgetContext.IsPreviewVisible` and `PreviewVisibilityChanged` to pause
preview-only work while another member is visible. Call `RequestAttention()`
when a completed timer, alert or similar event should bring your widget to the
top.

**Can users click things inside the preview, or only open the flyout?**
Both. Interactive controls in the preview receive their events (a button's
`Click` fires and does not open the flyout); clicks on non-interactive parts
open the flyout. Drag & drop onto the preview is native too. See
[[Preview, flyout & settings|Preview-Flyout-Settings]].

**I already ship a WinUI app. Do I need a separate widget package?**
No. Add a tiny widget executable to the package you already ship: your app
stays untouched, the widget shares the package (and its storage), and users get
it with the app. The walkthrough is [[Companion widgets|Companion-Widgets]].

**My flyout stops updating after I close and reopen it.**
The flyout content is created once and reused across opens, and `Unloaded`
fires at every close. Don't tear down subscriptions on `Unloaded`; pause timers
via `IWidgetFlyoutLifecycle` instead. Details in
[[the plugin contract|Plugin-Contract]].

**A picker closes my flyout. What should I do?**
Create a `WidgetFlyout.EnterModalScope()` before opening the picker and dispose
the scope after it closes. The flyout stays active while the modal window owns
focus.

**Can I use third-party or native libraries?**
Freely. You run in your own process, so add whatever you need; there's no shared
load context to conflict with WidBar or other widgets. They ship as ordinary app
payload.

**I changed my code but WidBar still shows the old version.**
Make sure you deployed or registered the package, not only built the project.
WidBar discovers installed AppExtensions, so the updated package must be
registered with Windows. If WidBar is already open, refresh the widget catalog or
reopen the Layout page. If deployment itself fails with a file-in-use error, stop
that widget instance or end its widget process, then deploy again.

**Running the exe shows a little info window instead of my widget.**
It was started without WidBar. Widgets only render inside the host; that window
is the SDK telling you so.

**How do I push an update?**
Bump `WidBarPluginVersion` (and the package version), rebuild, and resubmit to
the Store. Users get it through normal Store updates.

**Which architectures should I target?**
x64 and arm64, what Windows 11 actually runs on.

**How do I get into WidBar's in-app showcase?**
Open a PR adding your entry to
[andelby/winbar-showcase](https://github.com/andelby/winbar-showcase). Details on
[[Packaging & publishing|Packaging-and-Publishing]].

**Where are the logs again?**
`widbar-sdk.*.log` under your package's `LocalCache\Local\Temp`, and the host log
at `%LOCALAPPDATA%\...\Logs\widbar.log`. More in [[Debugging]].
