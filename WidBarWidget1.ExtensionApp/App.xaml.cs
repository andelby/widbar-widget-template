using WidBar.SDK;
using WidBar.SDK.Hosting;

namespace WidBarWidget1.ExtensionApp;

// Entry point of the plugin process. The SDK base class takes care of talking
// to WidBar, rendering the taskbar preview and hosting the flyout/settings
// windows. All we do here is hand it our plugin (one instance per active widget).
public partial class App : WidgetHostApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override IWidgetPlugin CreatePlugin() => new MainPlugin();
}
