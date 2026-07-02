using System;
using System.Windows;
using System.Windows.Media;
using FormsDialog = System.Windows.Forms.ColorDialog;

namespace 渔人的直感.Models
{
    public static class ColorPickerHelper
    {
        public static bool TryPickColor(string currentHex, Window owner, out string selectedHex)
        {
            selectedHex = currentHex;
            if (string.IsNullOrWhiteSpace(currentHex))
                currentHex = "#FFFFFF";

            Color initial;
            try
            {
                initial = (Color)ColorConverter.ConvertFromString(currentHex);
            }
            catch
            {
                initial = Colors.White;
            }

            using (var dialog = new FormsDialog())
            {
                dialog.FullOpen = true;
                dialog.Color = System.Drawing.Color.FromArgb(initial.A, initial.R, initial.G, initial.B);
                dialog.AnyColor = true;

                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return false;

                selectedHex = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}";
                return true;
            }
        }

        public static SolidColorBrush ToBrush(string hex, SolidColorBrush fallback = null)
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch
            {
                return fallback ?? new SolidColorBrush(Colors.White);
            }
        }

        public static void ApplyButtonColor(System.Windows.Controls.Button button, string hex)
        {
            if (button == null)
                return;

            button.Background = ToBrush(hex, new SolidColorBrush(Colors.White));
        }
    }
}
