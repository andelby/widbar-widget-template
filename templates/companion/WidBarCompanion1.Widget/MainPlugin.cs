using System.Text.Json;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WidBar.SDK;

namespace WidBarCompanion1.Widget;

// Sample companion widget: a clock on the taskbar, a bigger clock in the
// flyout and a 12/24h toggle in settings. Replace the UI with surfaces backed
// by your app's data. This code runs as a tiny second exe in your app package.
public sealed class MainPlugin : WidgetPluginBase, IConfigurableWidgetPlugin
{
    private Settings _settings = new();
    private TextBlock? _previewText;
    private Timer? _timer;

    public override string Id => "com.example.companion";
    public override string Name => "MY-COMPANION-DISPLAY-NAME";
    public override string Description => "Quick access to your app from the taskbar.";
    public override WidgetCategory Category => WidgetCategory.Utility;

    public override int PreviewLogicalWidth => 150;
    public override int FlyoutWidth => 360;
    public override int FlyoutHeight => 240;
    public override WidgetFlyoutBackdrop FlyoutBackdrop => WidgetFlyoutBackdrop.Acrylic;

    private sealed class Settings
    {
        public bool Use24h { get; set; } = true;

        public static Settings FromJson(string? json)
        {
            try
            {
                return string.IsNullOrWhiteSpace(json)
                    ? new Settings()
                    : JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        public string ToJson() => JsonSerializer.Serialize(this);
    }

    private string TimeText => DateTime.Now.ToString(_settings.Use24h ? "HH:mm:ss" : "hh:mm:ss tt");

    public override Task InitializeAsync(IWidgetContext context)
    {
        _settings = Settings.FromJson(context.SettingsJson);
        base.InitializeAsync(context);

        // Ask WidBar to refresh the taskbar preview once a second.
        _timer = new Timer(_ => Context?.RequestPreviewRefresh(), null,
            TimeSpan.Zero, TimeSpan.FromSeconds(1));

        return Task.CompletedTask;
    }

    // Taskbar preview. Return a compact WinUI element sized for a taskbar slot.
    // Hover, placement and click-to-open are handled on the WidBar side.
    public override UIElement? CreatePreviewContent()
    {
        _previewText = new TextBlock
        {
            Text = TimeText,
            FontSize = 16,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var root = new Grid();
        root.Children.Add(_previewText);
        root.Loaded += (_, _) => _previewText.Text = TimeText;

        // Keep the visible text fresh while the preview is alive.
        var refresh = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        refresh.Tick += (_, _) => _previewText!.Text = TimeText;
        refresh.Start();

        return root;
    }

    // Flyout shown when the user clicks the preview. This is a real window,
    // so anything goes: scrolling, input, Win2D, whatever you need.
    public override UIElement? CreateFlyoutContent()
    {
        var clock = new TextBlock
        {
            Text = TimeText,
            FontSize = 40,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => clock.Text = TimeText;
        timer.Start();

        var panel = new StackPanel
        {
            Spacing = 12,
            Padding = new Thickness(24),
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.Add(clock);
        panel.Children.Add(new TextBlock
        {
            Text = "MY-COMPANION-DISPLAY-NAME",
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.6,
        });

        return panel;
    }

    // Settings UI. The SDK hosts it in a window with Save/Cancel buttons and
    // opens it from the WidBar app or from the gear in the flyout. Call
    // SaveSettings on every change so the draft stays current.
    public UIElement? CreateSettingsContent(IWidgetSettingsContext context)
    {
        var draft = Settings.FromJson(context.SettingsJson);

        var toggle = new ToggleSwitch
        {
            Header = "Use 24-hour clock",
            IsOn = draft.Use24h,
        };
        toggle.Toggled += (_, _) =>
        {
            draft.Use24h = toggle.IsOn;
            context.SaveSettings(draft.ToJson());
        };

        var panel = new StackPanel { Spacing = 16 };
        panel.Children.Add(toggle);
        return panel;
    }

    // Called while the user edits settings (and again with the original JSON
    // if they cancel). Apply the draft so the taskbar preview updates live.
    public override void OnSettingsDraftChanged(string settingsJson)
    {
        _settings = Settings.FromJson(settingsJson);
        if (_previewText is not null)
        {
            _previewText.Text = TimeText;
        }
    }

    public override ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        _timer = null;
        return ValueTask.CompletedTask;
    }
}
