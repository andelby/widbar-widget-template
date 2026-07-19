# Building a WidBar widget

This is the developer documentation for writing widgets that run on the WidBar
taskbar: the `WidBar.SDK` package, the project templates, and the small amount of
Windows packaging you can't get around. If you've shipped a WinUI 3 app before,
almost none of this will surprise you. If you haven't, the templates get you to a
running widget before you have to understand any of the plumbing.

Quick note on who this is for. It's for people building widgets. If you just want
widgets on your taskbar, install WidBar from the Microsoft Store and you can
ignore everything here.

## How a widget actually works

A widget is an ordinary packaged WinUI 3 app with one twist: it never shows a
window of its own. WidBar starts your process, asks it for WinUI elements, and
places those elements in the right host surface: a compact taskbar preview, a
flyout, or a settings window. That's the whole model, more or less.

You write one class. It returns up to three pieces of UI:

- the preview that sits on the taskbar,
- the flyout popup shown on click, and
- a settings page, if your widget has anything to configure.

Everything else is WidBar's job, not yours: process lifetime, taskbar placement,
per-monitor DPI, settings storage, and restart/disable behaviour when a widget
keeps crashing.

The SDK also applies the Windows light or dark theme to each host surface.
Smart stack APIs let a widget pause preview-only work while hidden and request
attention when it has something timely to show.

One thing worth learning early: a single process serves every copy of your
widget. Drop it on two taskbars and `CreatePlugin()` runs twice in the same
process. So keep per-widget state in instance fields. The moment you reach for a
`static`, the two copies start fighting over it.

Widgets are discovered through a Windows AppExtension named `com.widbar.widget`.
The template wires that up for you. See
[[Packaging & publishing|Packaging-and-Publishing]] if you ever need to touch it.

## Two ways to build one

There are two equally supported paths, and they share the same plugin contract:

- **Standalone widget** (`widbar-widget`): a small dedicated package whose only job
  is being a widget. Right when the widget is the product.
- **Companion widget**: your *existing* app's package gains a tiny second
  executable that hosts the widget, sharing the package identity (and therefore
  storage) with the app. Right when you already ship an app and want a taskbar
  surface for it. The app itself doesn't change. See
  [[Companion widgets|Companion-Widgets]].

## Where to go next

- Brand new? Start with [[Getting started|Getting-Started]].
- Writing the plugin class? [[The plugin contract|Plugin-Contract]].
- Designing the UI? [[Preview, flyout & settings|Preview-Flyout-Settings]].
- Adding a widget to an app you already ship? [[Companion widgets|Companion-Widgets]].
- Ready to ship? [[Packaging & publishing|Packaging-and-Publishing]].
- Something not working? [[Debugging]] and the [[FAQ]].

## A note on these pages

They live in the template repo under `wiki/` and are mirrored here. Edit them in
the repo via PR if you spot something wrong; the wiki copy is generated from
there.

Useful links: [SDK on NuGet](https://www.nuget.org/packages/WidBar.SDK),
[template repo](https://github.com/andelby/widbar-widget-template),
[showcase catalog](https://github.com/andelby/winbar-showcase).
