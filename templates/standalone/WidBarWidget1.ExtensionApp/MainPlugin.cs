using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WidBar.SDK;

namespace WidBarWidget1.ExtensionApp;

// Sample widget: a clock on the taskbar, a bigger clock in the flyout and a
// 12/24h toggle in settings. Replace the UI with your own. This code runs in
// its own process, so feel free to pull in any NuGet or native dependency.
public sealed class MainPlugin :
    WidgetPluginBase,
    IConfigurableWidgetPlugin,
    IWidgetFlyoutLifecycle
{
    private Settings _settings = new();
    private TextBlock? _previewText;
    private TextBlock? _flyoutText;
    private DispatcherTimer? _previewTimer;
    private DispatcherTimer? _flyoutTimer;

    // Catalog metadata (description, category, version) lives in the .csproj
    // WidBarPlugin* properties -> plugin.json, the single source the WidBar
    // catalog reads. The plugin class only carries what the runtime needs.
    public override string Id => "com.example.mywidget";
    public override string Name => "MY-WIDGET-DISPLAY-NAME";

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

    public override async Task InitializeAsync(IWidgetContext context)
    {
        _settings = Settings.FromJson(context.SettingsJson);
        await base.InitializeAsync(context);
        context.PreviewVisibilityChanged += OnPreviewVisibilityChanged;
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
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var root = new Grid
        {
            Background = null,
        };
        root.Children.Add(_previewText);

        _previewTimer?.Stop();
        _previewTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _previewTimer.Tick += OnPreviewTimerTick;
        SetPreviewUpdatesEnabled(Context?.IsPreviewVisible ?? true);

        return root;
    }

    // Flyout shown when the user clicks the preview. This is a real window,
    // so anything goes: scrolling, input, Win2D, whatever you need.
    public override UIElement? CreateFlyoutContent()
    {
        _flyoutText = new TextBlock
        {
            Text = TimeText,
            FontSize = 40,
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        _flyoutTimer?.Stop();
        _flyoutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _flyoutTimer.Tick += OnFlyoutTimerTick;

        var panel = new StackPanel
        {
            Spacing = 12,
            Padding = new Thickness(24),
            VerticalAlignment = VerticalAlignment.Center,
        };
        panel.Children.Add(_flyoutText);
        panel.Children.Add(new TextBlock
        {
            Text = "MY-WIDGET-DISPLAY-NAME",
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
            context.RequestPreviewRefresh();
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

        if (_flyoutText is not null)
        {
            _flyoutText.Text = TimeText;
        }
    }

    public void OnFlyoutShown()
    {
        if (_flyoutText is not null)
        {
            _flyoutText.Text = TimeText;
        }

        _flyoutTimer?.Start();
    }

    public void OnFlyoutHidden()
    {
        _flyoutTimer?.Stop();
    }

    public override ValueTask DisposeAsync()
    {
        if (Context is not null)
        {
            Context.PreviewVisibilityChanged -= OnPreviewVisibilityChanged;
        }

        _previewTimer?.Stop();
        _flyoutTimer?.Stop();
        _previewTimer = null;
        _flyoutTimer = null;
        _previewText = null;
        _flyoutText = null;
        return ValueTask.CompletedTask;
    }

    private void OnPreviewVisibilityChanged(object? sender, bool isVisible)
    {
        SetPreviewUpdatesEnabled(isVisible);
    }

    private void SetPreviewUpdatesEnabled(bool isVisible)
    {
        if (isVisible)
        {
            if (_previewText is not null)
            {
                _previewText.Text = TimeText;
            }

            _previewTimer?.Start();
        }
        else
        {
            _previewTimer?.Stop();
        }
    }

    private void OnPreviewTimerTick(object? sender, object e)
    {
        if (_previewText is not null)
        {
            _previewText.Text = TimeText;
        }
    }

    private void OnFlyoutTimerTick(object? sender, object e)
    {
        if (_flyoutText is not null)
        {
            _flyoutText.Text = TimeText;
        }
    }
}
