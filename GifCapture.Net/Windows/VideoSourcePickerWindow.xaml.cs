using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using GifCapture.Base;
using GifCapture.Images;
using GifCapture.Models;
using GifCapture.Screen;
using GifCapture.Services;
using Color = System.Windows.Media.Color;
using Window = System.Windows.Window;

namespace GifCapture.Net.Windows
{
    public partial class VideoSourcePickerWindow : Window
    {
        Predicate<IWindow> Predicate { get; set; }

        VideoSourcePickerWindow()
        {
            InitializeComponent();

            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            UpdateBackground();

            var platformServices = ServiceProvider.IPlatformServices;
            _screens = platformServices.EnumerateScreens().ToArray();
            _windows = platformServices.EnumerateWindows().ToArray();

            ShowCancelText();
        }

        readonly IScreen[] _screens;

        readonly IWindow[] _windows;

        public IWindow SelectedWindow { get; private set; }

        void UpdateBackground()
        {
            using (IBitmapImage bmp = ScreenShot.Capture())
            {
                using (Stream stream = new MemoryStream())
                {
                    bmp.Save(stream, ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    this.Background = new ImageBrush(decoder.Frames[0]);
                }
            }
        }

        void ShowCancelText()
        {
            foreach (var screen in _screens)
            {
                var bounds = screen.Rectangle;

                var left = -Left + bounds.Left / Dpi.X;
                var top = -Top + bounds.Top / Dpi.Y;
                var width = bounds.Width / Dpi.X;
                var height = bounds.Height / Dpi.Y;

                var container = new ContentControl
                {
                    Width = width,
                    Height = height,
                    Margin = new Thickness(left, top, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                var textBlock = new TextBlock
                {
                    Text = Properties.Resources.SelectWindowTip,
                    FontSize = 15,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(10, 5, 10, 5),
                    Foreground = new SolidColorBrush(Colors.Black),
                    Background = new SolidColorBrush(Colors.White)
                };
                container.Content = textBlock;
                RootGrid.Children.Add(container);
            }
        }

        void CloseClick(object sender, RoutedEventArgs e)
        {
            SelectedWindow = null;
            Close();
        }

        Rectangle? _lastRectangle;

        void UpdateBorderAndCursor(Rectangle? rect)
        {
            if (_lastRectangle == rect)
            {
                return;
            }

            _lastRectangle = rect;
            if (rect == null)
            {
                Cursor = Cursors.Arrow;
                Border.Width = Border.Height = 0;
            }
            else
            {
                Cursor = Cursors.Hand;
                var r = rect.Value;

                var margin = new Thickness(-Left + r.Left / Dpi.X, -Top + r.Top / Dpi.Y, 0, 0);
                Border.Margin = margin;
                Border.Width = r.Width / Dpi.X;
                Border.Height = r.Height / Dpi.Y;
            }
        }

        void WindowMouseMove(object sender, MouseEventArgs e)
        {
            var platformServices = ServiceProvider.IPlatformServices;
            var point = platformServices.CursorPosition;
            SelectedWindow = _windows
                .Where(m => Predicate?.Invoke(m) ?? true)
                .FirstOrDefault(m => m.Rectangle.Contains(point));
            UpdateBorderAndCursor(SelectedWindow?.Rectangle);
        }

        void WindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedWindow != null)
            {
                Close();
            }
        }

        public static IWindow PickWindow()
        {
            var picker = new VideoSourcePickerWindow();
            picker.ShowDialog();
            return picker.SelectedWindow;
        }
    }
}