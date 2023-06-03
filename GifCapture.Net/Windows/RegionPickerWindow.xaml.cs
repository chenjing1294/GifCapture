using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GifCapture.Base;
using GifCapture.Net.Controls;
using GifCapture.Images;
using GifCapture.Models;
using GifCapture.Screen;
using GifCapture.Services;
using Point = System.Windows.Point;
using Window = System.Windows.Window;

namespace GifCapture.Net.Windows
{
    public partial class RegionPickerWindow : Window
    {
        private readonly IWindow[] _windows;
        private readonly IPlatformServices _platformServices;
        private Predicate<IWindow> Predicate { get; set; }

        public RegionPickerWindow()
        {
            InitializeComponent();
            _platformServices = ServiceProvider.IPlatformServices;
            _windows = _platformServices
                .EnumerateAllWindows()
                .ToArray();

            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            UpdateBackground();
        }


        void UpdateBackground()
        {
            using (IBitmapImage b = ScreenShot.Capture())
            {
                using (Stream stream = new MemoryStream())
                {
                    b.Save(stream, ImageFormat.Bmp);
                    stream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    BgImg.Source = bitmapImage;
                }
            }
        }

        void CloseClick(object sender, RoutedEventArgs e)
        {
            _start = _end = null;
            Close();
        }

        void UpdateSizeDisplay(Rect? rect)
        {
            if (rect == null)
            {
                SizeTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                Rect r = rect.Value;
                SizeTextBlock.Text = $"{(int) r.Width} x {(int) r.Height}";
                SizeTextBlock.Margin = new Thickness(
                    r.Left + r.Width / 2 - SizeTextBlock.ActualWidth / 2,
                    r.Top + r.Height / 2 - SizeTextBlock.ActualHeight / 2,
                    0,
                    0);
                SizeTextBlock.Visibility = Visibility.Visible;

                System.Drawing.Point point = _platformServices.CursorPosition;
                Magnifier.UpdatePositionTextBlock(new Point(point.X, point.Y), new System.Windows.Size(r.Width, r.Height));
            }
        }

        void WindowMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _end = e.GetPosition(RootGrid);
                Rect? r = GetRegion();
                UpdateSizeDisplay(r);
                if (r == null)
                {
                    Unhighlight();
                    return;
                }

                HighlightRegion(r.Value);
                UpdateViewBox(_platformServices.CursorPosition);
                Magnifier.HideRectangle();
            }
            else
            {
                System.Drawing.Point point = _platformServices.CursorPosition;
                _selectedWindow = _windows
                    .Where(w => Predicate?.Invoke(w) ?? true)
                    .FirstOrDefault(w => w.Rectangle.Contains(point));

                if (_selectedWindow == null)
                {
                    UpdateSizeDisplay(null);
                    Unhighlight();
                }
                else
                {
                    Rect? rect = GetSelectedWindowRectangle();
                    Debug.Assert(rect != null, nameof(rect) + " != null");
                    Rect r = rect.Value;
                    UpdateSizeDisplay(r);
                    HighlightRegion(r);
                    UpdateViewBox(point);
                    Magnifier.ShowRectangle();
                }
            }
        }

        void UpdateViewBox(System.Drawing.Point point)
        {
            Magnifier.UpdateViewBox(new System.Windows.Point(point.X - 25, point.Y - 25), new System.Windows.Size(51, 51));
        }

        Rect? GetSelectedWindowRectangle()
        {
            if (_selectedWindow == null)
            {
                return null;
            }

            var rect = _selectedWindow.Rectangle;
            return new Rect(
                -Left + rect.X / Dpi.X,
                -Top + rect.Y / Dpi.Y,
                rect.Width / Dpi.X,
                rect.Height / Dpi.Y);
        }

        bool _isDragging;
        Point? _start, _end;
        IWindow _selectedWindow;

        void WindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _start = e.GetPosition(RootGrid);
            _end = null;
        }

        void WindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging)
                return;

            var current = e.GetPosition(RootGrid);

            if (current != _start)
            {
                _end = e.GetPosition(RootGrid);
            }
            else if (GetSelectedWindowRectangle() is Rect rect)
            {
                _start = rect.Location;
                _end = new Point(rect.Right, rect.Bottom);
            }

            Close();
        }

        /// <summary>
        /// 计算鼠标框选的区域
        /// </summary>
        Rect? GetRegion()
        {
            if (_start == null || _end == null)
            {
                return null;
            }

            Point end = _end.Value;
            Point start = _start.Value;

            if (end.X < start.X)
            {
                (start.X, end.X) = (end.X, start.X);
            }

            if (end.Y < start.Y)
            {
                (start.Y, end.Y) = (end.Y, start.Y);
            }

            double width = end.X - start.X;
            double height = end.Y - start.Y;

            if (width < 0.01 || height < 0.01)
            {
                return null;
            }

            return new Rect(start.X, start.Y, width, height);
        }

        private Rectangle? GetRegionScaled()
        {
            var rect = GetRegion();
            if (rect == null)
            {
                return null;
            }

            Rect r = rect.Value;
            return new Rectangle(
                (int) ((this.Left + r.X) * Dpi.X),
                (int) ((this.Top + r.Y) * Dpi.Y),
                (int) (r.Width * Dpi.X),
                (int) (r.Height * Dpi.Y));
        }

        public static Rectangle? PickRegion()
        {
            var picker = new RegionPickerWindow();
            picker.ShowDialog();
            return picker.GetRegionScaled();
        }

        void Unhighlight()
        {
            StripedBorder.Visibility = Visibility.Collapsed;
            PuncturedRegion.Region = null;
        }

        void HighlightRegion(Rect region)
        {
            StripedBorder.Margin = new Thickness(region.X, region.Y, 0, 0);
            StripedBorder.Width = region.Width;
            StripedBorder.Height = region.Height;
            PuncturedRegion.Region = region;
            StripedBorder.Visibility = Visibility.Visible;
        }

        private void Magnifier_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Magnifier magnifier)
            {
                if (magnifier.HorizontalAlignment == HorizontalAlignment.Right && magnifier.VerticalAlignment == VerticalAlignment.Bottom)
                {
                    magnifier.HorizontalAlignment = HorizontalAlignment.Left;
                    magnifier.VerticalAlignment = VerticalAlignment.Top;
                }
                else
                {
                    magnifier.HorizontalAlignment = HorizontalAlignment.Right;
                    magnifier.VerticalAlignment = VerticalAlignment.Bottom;
                }
            }
        }
    }
}