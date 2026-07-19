# Packaging & publishing

WidBar finds widgets through a Windows AppExtension. The template sets all of
this up, so most of the time you won't touch it, but it's worth understanding the
three pieces so you know what you're changing when you do.

## plugin.json, generated

Your widget advertises itself with a small `plugin.json`. You never write it: the
SDK build target generates `obj\widbar\plugin.json` from MSBuild properties in
the ExtensionApp `.csproj`. Edit the properties and the JSON follows.

```xml
<PropertyGroup>
  <WidBarPluginId>com.contoso.weather</WidBarPluginId>
  <WidBarPluginName>Weather</WidBarPluginName>
  <WidBarPluginDescription>Compact weather widget.</WidBarPluginDescription>
  <WidBarPluginCategory>Information</WidBarPluginCategory>
  <WidBarPluginVersion>1.0.0</WidBarPluginVersion>
  <WidBarPluginPreviewWidth>188</WidBarPluginPreviewWidth>
  <WidBarPluginConfigurable>true</WidBarPluginConfigurable>
</PropertyGroup>
```

Keep `WidBarPluginId` identical to the `Id` your plugin class returns. If they
drift, WidBar won't match the catalog entry to the running plugin.

Description and category live only in this project metadata. Do not add runtime
`Description` or `Category` overrides to the plugin class. Those properties are
not part of the SDK 2.0 runtime contract.

## The package manifest

`Package.appxmanifest` in the packaging project declares the AppExtension that
makes you discoverable:

```xml
<uap3:Extension Category="windows.appExtension">
  <uap3:AppExtension Name="com.widbar.widget" Id="weather"
                     PublicFolder="Public" DisplayName="Weather">
    <uap3:Properties>
      <WidBarPluginId>com.contoso.weather</WidBarPluginId>
      <WidBarSdkVersion>2</WidBarSdkVersion>
    </uap3:Properties>
  </uap3:AppExtension>
</uap3:Extension>
```

`Name="com.widbar.widget"` is the contract WidBar scans for, so leave it exactly
that. `WidBarSdkVersion` is the host contract version (currently `2`). And you'll
need `runFullTrust` in your capabilities.

The only thing that goes in the `Public` folder is the generated manifest, linked
in from the wapproj:

```xml
<Content Include="..\Contoso.Weather.ExtensionApp\obj\widbar\plugin.json">
  <Link>Public\plugin.json</Link>
</Content>
```

Your actual code, every DLL, managed or native, ships as normal app payload of
the executable. Nothing special, no shared probing, no version fights with other
widgets, because you're in your own process.

Keep all dependency files beside the widget executable. If a XAML control
library includes resource dictionaries, merge them from the widget application
definition before creating preview or flyout content.

## Architectures

Build for the architectures Windows 11 runs on: x64 and arm64. A Store bundle
built in `StoreUpload` mode gives you a single `.msixupload` that carries both.

## Getting onto the Store

Reserve your app name in Partner Center and point the packaging project's
Identity and Publisher at it (*Associate App with the Store* in Visual Studio
does this). Build Release, which produces the `.msixupload`, and upload it under
your app's *Packages* in
[Partner Center](https://partner.microsoft.com/dashboard). The Store re-signs the
package, so you don't ship signing certificates yourself.

## Showing up in WidBar's showcase

WidBar has an in-app showcase backed by a public catalog. Listing is optional and
free: open a PR against
[andelby/winbar-showcase](https://github.com/andelby/winbar-showcase) with an
entry for your widget.

```json
{
  "id": "contoso-weather",
  "pluginId": "com.contoso.weather",
  "name": "Weather",
  "summary": "Compact weather, right on the taskbar.",
  "publisher": "Contoso",
  "category": "system",
  "storeProductId": "9NXXXXXXXXXX",
  "iconUrl": "https://.../weather.png",
  "localizations": { "it-IT": { "name": "Meteo", "summary": "..." } }
}
```

`pluginId` has to match your widget's `Id`, since that's what WidBar uses to show
an "Installed" badge, and `storeProductId` is your Store product ID.
