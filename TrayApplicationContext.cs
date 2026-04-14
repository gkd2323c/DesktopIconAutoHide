using System.Drawing;

namespace DesktopIconAutoHide;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly ToolStripMenuItem _showIconsMenuItem;
    private readonly ToolStripMenuItem _hideIconsMenuItem;
    private readonly ToolStripMenuItem _settingsMenuItem;
    private readonly ToolStripMenuItem _exitMenuItem;
    private TimeSpan _idleThreshold;

    private Point _lastCursorPosition;
    private DateTime _lastMouseMoveUtc;
    private bool _iconsHidden;
    private bool _hiddenByApp;

    internal TrayApplicationContext()
    {
        var settings = AppSettingsStore.Load();
        LocalizedText.SetLanguageMode(settings.LanguageMode);
        _idleThreshold = TimeSpan.FromSeconds(settings.IdleSeconds);

        _lastCursorPosition = Cursor.Position;
        _lastMouseMoveUtc = DateTime.UtcNow;
        var visibleState = DesktopIconController.AreDesktopIconsVisible();
        _iconsHidden = visibleState.HasValue && !visibleState.Value;

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = BuildIdleTooltipText()
        };
        _notifyIcon.DoubleClick += (_, _) => ShowIconsAndResetTimer();

        var menu = new ContextMenuStrip();
        _showIconsMenuItem = new ToolStripMenuItem();
        _showIconsMenuItem.Click += (_, _) => ShowIconsAndResetTimer();

        _hideIconsMenuItem = new ToolStripMenuItem();
        _hideIconsMenuItem.Click += (_, _) => HideIconsIfDesktop();

        _settingsMenuItem = new ToolStripMenuItem();
        _settingsMenuItem.Click += (_, _) => OpenSettings();

        _exitMenuItem = new ToolStripMenuItem();
        _exitMenuItem.Click += (_, _) => ExitThread();

        menu.Items.Add(_showIconsMenuItem);
        menu.Items.Add(_hideIconsMenuItem);
        menu.Items.Add(_settingsMenuItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_exitMenuItem);
        _notifyIcon.ContextMenuStrip = menu;
        RefreshLocalizedText();

        _timer = new System.Windows.Forms.Timer
        {
            Interval = 500
        };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var nowUtc = DateTime.UtcNow;
        var cursorPosition = Cursor.Position;

        if (cursorPosition != _lastCursorPosition)
        {
            _lastCursorPosition = cursorPosition;
            _lastMouseMoveUtc = nowUtc;

            if (_iconsHidden && _hiddenByApp)
            {
                SetIconsVisible(true);
            }

            return;
        }

        if (!DesktopIconController.IsDesktopForeground())
        {
            if (_iconsHidden && _hiddenByApp)
            {
                SetIconsVisible(true);
            }

            return;
        }

        var idleDuration = nowUtc - _lastMouseMoveUtc;
        if (!_iconsHidden && idleDuration >= _idleThreshold)
        {
            SetIconsVisible(false);
        }
    }

    private void ShowIconsAndResetTimer()
    {
        _lastMouseMoveUtc = DateTime.UtcNow;
        _lastCursorPosition = Cursor.Position;
        SetIconsVisible(true);
    }

    private void HideIconsIfDesktop()
    {
        if (DesktopIconController.IsDesktopForeground())
        {
            SetIconsVisible(false);
        }
    }

    private void SetIconsVisible(bool visible)
    {
        if (!DesktopIconController.SetDesktopIconsVisible(visible))
        {
            return;
        }

        _iconsHidden = !visible;
        _hiddenByApp = !visible;
        _notifyIcon.Text = visible
            ? BuildIdleTooltipText()
            : LocalizedText.HiddenTooltip;
    }

    private string BuildIdleTooltipText() =>
        LocalizedText.BuildIdleTooltip((int)_idleThreshold.TotalSeconds);

    private void OpenSettings()
    {
        using var form = new SettingsForm((int)_idleThreshold.TotalSeconds, LocalizedText.LanguageMode);
        if (form.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        LocalizedText.SetLanguageMode(form.LanguageMode);
        _idleThreshold = TimeSpan.FromSeconds(form.IdleSeconds);
        AppSettingsStore.Save(new AppSettings(form.IdleSeconds, form.LanguageMode));
        RefreshLocalizedText();

        _lastCursorPosition = Cursor.Position;
        _lastMouseMoveUtc = DateTime.UtcNow;

        if (_iconsHidden && _hiddenByApp)
        {
            SetIconsVisible(true);
        }
        else
        {
            _notifyIcon.Text = BuildIdleTooltipText();
        }
    }

    private void RefreshLocalizedText()
    {
        _showIconsMenuItem.Text = LocalizedText.MenuShowNow;
        _hideIconsMenuItem.Text = LocalizedText.MenuHideNow;
        _settingsMenuItem.Text = LocalizedText.MenuSettings;
        _exitMenuItem.Text = LocalizedText.MenuExit;
        _notifyIcon.Text = _iconsHidden ? LocalizedText.HiddenTooltip : BuildIdleTooltipText();
    }

    protected override void ExitThreadCore()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
        _timer.Dispose();

        if (_iconsHidden && _hiddenByApp)
        {
            _ = DesktopIconController.SetDesktopIconsVisible(true);
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();

        base.ExitThreadCore();
    }
}
