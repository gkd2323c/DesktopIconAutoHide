using System.Drawing;

namespace DesktopIconAutoHide;

internal sealed class SettingsForm : Form
{
    private readonly NumericUpDown _idleSecondsInput;
    private readonly ComboBox _languageInput;
    private readonly CheckBox _autoStartCheckbox;

    internal int IdleSeconds => (int)_idleSecondsInput.Value;
    internal bool AutoStartEnabled => _autoStartCheckbox.Checked;
    internal AppLanguageMode LanguageMode =>
        _languageInput.SelectedItem is LanguageOption option
            ? option.Mode
            : AppLanguageMode.Auto;

    internal SettingsForm(
        int currentIdleSeconds,
        AppLanguageMode currentLanguageMode,
        bool currentAutoStartEnabled)
    {
        Text = LocalizedText.SettingsTitle;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(420, 220);

        var titleLabel = new Label
        {
            AutoSize = true,
            Location = new Point(12, 16),
            Text = LocalizedText.SettingsIdleLabel
        };

        _idleSecondsInput = new NumericUpDown
        {
            Minimum = AppSettingsStore.MinIdleSeconds,
            Maximum = AppSettingsStore.MaxIdleSeconds,
            Value = Math.Clamp(
                currentIdleSeconds,
                AppSettingsStore.MinIdleSeconds,
                AppSettingsStore.MaxIdleSeconds),
            Location = new Point(15, 42),
            Width = 120
        };

        var languageLabel = new Label
        {
            AutoSize = true,
            Location = new Point(12, 78),
            Text = LocalizedText.SettingsLanguageLabel
        };

        _languageInput = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(15, 104),
            Width = 160
        };
        _languageInput.Items.Add(new LanguageOption(AppLanguageMode.Auto, LocalizedText.LanguageOptionAuto));
        _languageInput.Items.Add(new LanguageOption(AppLanguageMode.Chinese, LocalizedText.LanguageOptionChinese));
        _languageInput.Items.Add(new LanguageOption(AppLanguageMode.English, LocalizedText.LanguageOptionEnglish));
        _languageInput.SelectedItem = _languageInput.Items
            .OfType<LanguageOption>()
            .FirstOrDefault(x => x.Mode == currentLanguageMode)
            ?? _languageInput.Items[0];

        _autoStartCheckbox = new CheckBox
        {
            AutoSize = true,
            Location = new Point(15, 146),
            Text = LocalizedText.SettingsAutoStartLabel,
            Checked = currentAutoStartEnabled
        };

        var saveButton = new Button
        {
            Text = LocalizedText.SettingsSave,
            DialogResult = DialogResult.OK,
            Location = new Point(252, 176),
            Width = 75
        };

        var cancelButton = new Button
        {
            Text = LocalizedText.SettingsCancel,
            DialogResult = DialogResult.Cancel,
            Location = new Point(333, 176),
            Width = 75
        };

        Controls.Add(titleLabel);
        Controls.Add(_idleSecondsInput);
        Controls.Add(languageLabel);
        Controls.Add(_languageInput);
        Controls.Add(_autoStartCheckbox);
        Controls.Add(saveButton);
        Controls.Add(cancelButton);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private sealed class LanguageOption
    {
        internal LanguageOption(AppLanguageMode mode, string text)
        {
            Mode = mode;
            Text = text;
        }

        internal AppLanguageMode Mode { get; }
        internal string Text { get; }

        public override string ToString() => Text;
    }
}
