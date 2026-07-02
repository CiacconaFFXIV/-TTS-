using System;
using System.Windows;
using System.Windows.Controls;
using 渔人的直感.Models;

namespace 渔人的直感
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            TtsTimingSlider.Minimum = -TtsService.PromptDurationMs;
            TtsTimingSlider.Maximum = TtsService.BiteSoundDurationMs;
            TtsTimingSlider.IsSnapToTickEnabled = false;
            RefreshColorButtons();
        }

        private void RefreshColorButtons()
        {
            var settings = Properties.Settings.Default;
            ColorPickerHelper.ApplyButtonColor(TimerColorButton, settings.TimerColor);
            ColorPickerHelper.ApplyButtonColor(LTugColorButton, settings.LTugColor);
            ColorPickerHelper.ApplyButtonColor(MTugColorButton, settings.MTugColor);
            ColorPickerHelper.ApplyButtonColor(HTugColorButton, settings.HTugColor);
            ColorPickerHelper.ApplyButtonColor(StatusColorButton, settings.StatusColor);
            ColorPickerHelper.ApplyButtonColor(FishIntuitionColorButton, settings.FishIntuitionColor);
            ColorPickerHelper.ApplyButtonColor(FishIntuitionShadowColorButton, settings.FishIntuitionShadowColor);
        }

        private void TimerColorButton_Click(object sender, RoutedEventArgs e) =>
            PickAndSetColor(v => Properties.Settings.Default.TimerColor = v, Properties.Settings.Default.TimerColor, TimerColorButton);

        private void LTugColorButton_Click(object sender, RoutedEventArgs e) =>
            PickAndSetColor(v => Properties.Settings.Default.LTugColor = v, Properties.Settings.Default.LTugColor, LTugColorButton);

        private void MTugColorButton_Click(object sender, RoutedEventArgs e) =>
            PickAndSetColor(v => Properties.Settings.Default.MTugColor = v, Properties.Settings.Default.MTugColor, MTugColorButton);

        private void HTugColorButton_Click(object sender, RoutedEventArgs e) =>
            PickAndSetColor(v => Properties.Settings.Default.HTugColor = v, Properties.Settings.Default.HTugColor, HTugColorButton);

        private void StatusColorButton_Click(object sender, RoutedEventArgs e) =>
            PickAndSetColor(v => Properties.Settings.Default.StatusColor = v, Properties.Settings.Default.StatusColor, StatusColorButton);

        private void FishIntuitionColorButton_Click(object sender, RoutedEventArgs e) =>
            PickAndSetColor(v => Properties.Settings.Default.FishIntuitionColor = v, Properties.Settings.Default.FishIntuitionColor, FishIntuitionColorButton);

        private void FishIntuitionShadowColorButton_Click(object sender, RoutedEventArgs e) =>
            PickAndSetColor(v => Properties.Settings.Default.FishIntuitionShadowColor = v, Properties.Settings.Default.FishIntuitionShadowColor, FishIntuitionShadowColorButton);

        private void PickAndSetColor(Action<string> setColor, string currentColor, Button button)
        {
            if (!ColorPickerHelper.TryPickColor(currentColor, this, out var selected))
                return;

            setColor(selected);
            ColorPickerHelper.ApplyButtonColor(button, selected);
        }

        private void Apply_Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            MainWindow.CurrentMainWindow?.ApplySettings();
            MessageBox.Show("设置已保存并已生效。", "渔人的直感", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Reset_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentMainWindow.Left = -1;
            MainWindow.CurrentMainWindow.Top = -1;
            Properties.Settings.Default.Reset();
            RefreshColorButtons();
            MainWindow.CurrentMainWindow?.ApplySettings();
        }
    }
}
