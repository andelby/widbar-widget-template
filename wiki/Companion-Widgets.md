# Companion widgets: ship a widget with an existing app

If you already ship a packaged WinUI 3 app, you don't need a separate widget
package on the Store. Add a tiny second executable to the package you already
have: your app keeps its own process, completely untouched, and WidBar launches
the small widget exe exactly like it launches any other widget. One package,
one installer, one Store listing; the widget arrives with the app.

The shape:

```text
MyApp (Package)/          your existing MSIX packaging project
  MyApp/                  your app, NOT touched
  MyApp.Widget/           new: a tiny widget host exe (this page)
```

Why a second exe instead of hosting the widget inside MyApp.exe itself? Process
hygiene. Your app's startup path drags in everything it needs (WebView2,
services, window plumbing); a widget host that boots all of that to draw a
taskbar slot is heavy and confusing in Task Manager. The widget exe references
only what the widget needs, starts in a blink, and your app's single-instance
logic never has to learn about widgets.

Because both exes live in the same package, they share the package identity,
including `LocalState` and `LocalSettings`. Your widget can read data your app
writes there. For code reuse, put shared models and services in a class library
that both projects reference.

## Fast start

Install the templates from the repo root:

```powershell
dotnet new install .
```

Then create the companion widget project beside your existing app project:

```powershell
dotnet new widbar-companion -n MyApp `
    --PluginId com.contoso.myapp.companion `
    --DisplayName "My App"
```

This creates `MyApp.Widget/`. Add that project to your existing solution, then
continue with the packaging and manifest steps below.

## Step 1: the widget project

The `widbar-companion` template creates a `MyApp.Widget` project next to your
app project. It is the same small WidBar widget process used by standalone
widgets, but without a packaging project of its own:

`MyApp.Widget.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <UseWinUI>true</UseWinUI>
    <WinUISDKReferences>false</WinUISDKReferences>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <Platforms>x64;ARM64</Platforms>
    <RuntimeIdentifiers>win-x64;win-arm64</RuntimeIdentifiers>
    <WindowsAppSDKSelfContained>false</WindowsAppSDKSelfContained>
    <DefineConstants>$(DefineConstants);DISABLE_XAML_GENERATED_MAIN</DefineConstants>
  </PropertyGroup>

  <PropertyGroup>
    <WidBarPluginId>com.contoso.myapp.companion</WidBarPluginId>
    <WidBarPluginName>My App</WidBarPluginName>
    <WidBarPluginDescription>Quick access to My App from the taskbar.</WidBarPluginDescription>
    <WidBarPluginCategory>Utility</WidBarPluginCategory>
    <WidBarPluginVersion>1.0.0</WidBarPluginVersion>
    <WidBarPluginPreviewWidth>132</WidBarPluginPreviewWidth>
    <WidBarPluginConfigurable>false</WidBarPluginConfigurable>
  </PropertyGroup>

  <!-- The application definition is WidgetApp.xaml, NOT the conventional
       App.xaml: your app already ships an App.xbf at the payload root, and
       two same-named .xbf files collide at packaging time. -->
  <ItemGroup>
    <ApplicationDefinition Include="WidgetApp.xaml" />
    <Page Remove="WidgetApp.xaml" />
  </ItemGroup>

  <ItemGroup>
    <Manifest Include="$(ApplicationManifest)" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="2.2.0" />
    <PackageReference Include="WidBar.SDK" Version="2.0.0" />
    <!-- optionally: <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" /> -->
  </ItemGroup>
</Project>
```

If your app package also targets x86, add `x86` / `win-x86` to the widget
project too. The widget project and the packaging project should agree on the
platforms you build.

`WidgetApp.xaml` and `WidgetApp.xaml.cs` derive from the SDK's host application:

```xml
<hosting:WidgetHostApplication
    x:Class="Contoso.MyApp.Widget.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:hosting="using:WidBar.SDK.Hosting">
    <hosting:WidgetHostApplication.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </hosting:WidgetHostApplication.Resources>
</hosting:WidgetHostApplication>
```

```csharp
public partial class App : WidgetHostApplication
{
    public App() { InitializeComponent(); }
    protected override IWidgetPlugin CreatePlugin() => new MyCompanionPlugin();
}
```

Add a `Program.cs` entry point. This keeps the companion widget on the same
startup path as normal WidBar widgets instead of using the XAML compiler's
generated `Main`:

```csharp
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WinRT;

namespace Contoso.MyApp.Widget;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        ComWrappersSupport.InitializeComWrappers();

        Application.Start(_ =>
        {
            var context = new DispatcherQueueSynchronizationContext(
                DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });
    }
}
```

Plus an `app.manifest` (PerMonitorV2 DPI awareness; copy the template's) and
your plugin class, which is the ordinary contract from
[[the plugin contract|Plugin-Contract]].

## Step 2: wire it into your packaging project

Your existing `.wapproj` gains a project reference and the plugin.json link:

```xml
<ProjectReference Include="..\MyApp.Widget\MyApp.Widget.csproj">
  <SkipGetTargetFrameworkProperties>True</SkipGetTargetFrameworkProperties>
  <PublishProfile>Properties\PublishProfiles\win-$(Platform).pubxml</PublishProfile>
</ProjectReference>

<Content Include="..\MyApp.Widget\obj\widbar\plugin.json">
  <Link>Public\plugin.json</Link>
</Content>
```

(Give the widget project the same `Properties\PublishProfiles\*.pubxml` files
your app project uses; the wapproj publishes every referenced exe the same way.)
Let the package project harvest the widget project output as its own payload
folder. Don't also copy `MyApp.Widget.exe`, `.deps.json`, or `.runtimeconfig.json`
to the package root; the widget process must start from the folder that contains
its own dlls, xbf files and dependency assets.

Two pitfalls that both produce confusing packaging errors:

- **Add the widget project to your solution.** A wapproj can only harvest
  projects the solution knows about; otherwise you get "Unable to find project
  information for ...Widget.csproj" at restore and "Manifest references file
  '...Widget.exe' which is not part of the payload" at pack.
- **Use the same Windows App SDK version in the app and the widget.** Both
  outputs merge into one payload; two different WinAppSDK versions mean
  same-named files with different content, and packaging rejects that. The
  widget needs 2.2 or later (the WidBar.SDK dependency), so align the app to
  it.
- **Don't call the widget's application definition App.xaml.** Both projects
  would emit an `App.xbf` at the payload root ("two or more files with the
  same destination path"). Name it `WidgetApp.xaml` and declare it with the
  `ApplicationDefinition` item shown above; identical framework files, by
  contrast, dedupe fine.

## Step 3: declare the widget as a second Application

This is the step that makes the widget start standalone. WidBar launches
installed widgets by activating the Application that DECLARES the
`com.widbar.widget` AppExtension. If you hang the extension off your main
app's `<Application>`, WidBar would boot your whole app as the widget host.
So the widget exe gets its own `<Application>` entry, hidden from the Start
menu, and the extension lives under it:

```xml
<Application Id="Widget"
    Executable="MyApp.Widget\MyApp.Widget.exe"
    EntryPoint="Windows.FullTrustApplication">
  <uap:VisualElements
      DisplayName="My App Widget"
      Description="My App taskbar widget"
      BackgroundColor="transparent"
      AppListEntry="none"
      Square150x150Logo="Images\Square150x150Logo.png"
      Square44x44Logo="Images\Square44x44Logo.png" />
  <Extensions>
    <uap3:Extension Category="windows.appExtension">
      <uap3:AppExtension Name="com.widbar.widget" Id="myappcompanion"
                         PublicFolder="Public" DisplayName="My App"
                         Description="Quick access to My App from the taskbar.">
        <uap3:Properties>
          <WidBarPluginId>com.contoso.myapp.companion</WidBarPluginId>
          <WidBarSdkVersion>2</WidBarSdkVersion>
        </uap3:Properties>
      </uap3:AppExtension>
    </uap3:Extension>
  </Extensions>
</Application>
```

Add it next to your existing `<Application>` inside `<Applications>` (and the
`uap3` namespace at the top if it's missing). `AppListEntry="none"` keeps the
widget out of the Start menu and the app list; reuse your app's logo assets
for the required VisualElements attributes. Multi-application packages are
fully supported by MSIX and the Store.

## Talking to your app

- **Launch or focus the app** from the flyout: both exes sit side by side in
  the package payload, so start the sibling directly. If your app is
  single-instanced, this focuses the running window instead of opening a
  duplicate:

  ```csharp
  var exe = Path.Combine(AppContext.BaseDirectory, "MyApp.exe");
  Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
  ```

- **Share data**: same package identity means the same
  `ApplicationData.Current.LocalFolder` and `LocalSettings`. Have the app write
  what the widget shows (recent items, counters, state) and the widget read it;
  refresh on a timer or on flyout open. No sockets, no pipes.

- **Share code**: move the models and services both sides need into a class
  library and reference it from both projects. Keep app-window assumptions out
  of it; the widget process has no main window.

## Deploy and test

Build and deploy the packaging project as usual. The widget appears in WidBar's
catalog next to every other widget. Use Refresh on the Layout page after a
redeploy if the new build is not discovered immediately.

The widget uses the same SDK 2.0 contract as a standalone widget, including
automatic theme support, smart stack visibility and attention requests.
