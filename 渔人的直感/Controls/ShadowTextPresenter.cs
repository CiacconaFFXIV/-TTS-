using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace 渔人的直感.Controls
{
    public class ShadowTextPresenter : UserControl
    {
        private readonly StackPanel _charsPanel;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ShadowTextPresenter),
                new PropertyMetadata(string.Empty, OnVisualPropertyChanged));

        public static readonly DependencyProperty TextFontSizeProperty =
            DependencyProperty.Register(nameof(TextFontSize), typeof(double), typeof(ShadowTextPresenter),
                new PropertyMetadata(14.0, OnVisualPropertyChanged));

        public static readonly DependencyProperty TextFontFamilyProperty =
            DependencyProperty.Register(nameof(TextFontFamily), typeof(FontFamily), typeof(ShadowTextPresenter),
                new PropertyMetadata(new FontFamily("Microsoft YaHei UI"), OnVisualPropertyChanged));

        public static readonly DependencyProperty TextForegroundProperty =
            DependencyProperty.Register(nameof(TextForeground), typeof(Brush), typeof(ShadowTextPresenter),
                new PropertyMetadata(Brushes.White, OnVisualPropertyChanged));

        public static readonly DependencyProperty ShadowColorProperty =
            DependencyProperty.Register(nameof(ShadowColor), typeof(Color), typeof(ShadowTextPresenter),
                new PropertyMetadata(Colors.Black, OnVisualPropertyChanged));

        public static readonly DependencyProperty ShadowBlurRadiusProperty =
            DependencyProperty.Register(nameof(ShadowBlurRadius), typeof(double), typeof(ShadowTextPresenter),
                new PropertyMetadata(3.0, OnVisualPropertyChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public double TextFontSize
        {
            get => (double)GetValue(TextFontSizeProperty);
            set => SetValue(TextFontSizeProperty, value);
        }

        public FontFamily TextFontFamily
        {
            get => (FontFamily)GetValue(TextFontFamilyProperty);
            set => SetValue(TextFontFamilyProperty, value);
        }

        public Brush TextForeground
        {
            get => (Brush)GetValue(TextForegroundProperty);
            set => SetValue(TextForegroundProperty, value);
        }

        public Color ShadowColor
        {
            get => (Color)GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        public double ShadowBlurRadius
        {
            get => (double)GetValue(ShadowBlurRadiusProperty);
            set => SetValue(ShadowBlurRadiusProperty, value);
        }

        public ShadowTextPresenter()
        {
            Background = Brushes.Transparent;
            _charsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Content = _charsPanel;
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ShadowTextPresenter)d).Rebuild();
        }

        private void Rebuild()
        {
            _charsPanel.Children.Clear();

            if (string.IsNullOrEmpty(Text))
                return;

            foreach (var ch in Text)
            {
                var block = new TextBlock
                {
                    Text = ch.ToString(),
                    FontSize = TextFontSize,
                    FontFamily = TextFontFamily,
                    Foreground = TextForeground,
                    Effect = new DropShadowEffect
                    {
                        ShadowDepth = 0,
                        BlurRadius = ShadowBlurRadius,
                        Color = ShadowColor,
                        Opacity = 0.9
                    }
                };
                _charsPanel.Children.Add(block);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _charsPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return _charsPanel.DesiredSize;
        }
    }
}
