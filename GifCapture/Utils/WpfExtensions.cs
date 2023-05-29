using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using GifCapture.Models;
using Microsoft.Win32;
using Reactive.Bindings;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using DColor = System.Drawing.Color;

namespace GifCapture.Utils
{
    public static class WpfExtensions
    {
        public static void ShowAndFocus(this Window w)
        {
            if (w.IsVisible && w.WindowState == WindowState.Minimized)
            {
                w.WindowState = WindowState.Normal;
            }

            w.Show();

            w.Activate();
        }

        public static Rectangle ApplyDpi(this RectangleF rectangle)
        {
            return new Rectangle((int) (rectangle.Left * Dpi.X),
                (int) (rectangle.Top * Dpi.Y),
                (int) (rectangle.Width * Dpi.X),
                (int) (rectangle.Height * Dpi.Y));
        }

        public static DColor ToDrawingColor(this Color C)
        {
            return DColor.FromArgb(C.A, C.R, C.G, C.B);
        }

        public static Color ToWpfColor(this DColor C)
        {
            return Color.FromArgb(C.A, C.R, C.G, C.B);
        }

        public static Color ParseColor(string S)
        {
            if (ColorConverter.ConvertFromString(S) is Color c)
                return c;

            return Colors.Transparent;
        }

        public static void Shake(this FrameworkElement element)
        {
            element.Dispatcher.Invoke(() =>
            {
                var transform = new TranslateTransform();
                element.RenderTransform = transform;

                const int delta = 5;

                var animation = new DoubleAnimationUsingKeyFrames
                {
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(1),
                    Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                    KeyFrames =
                    {
                        new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0)),
                        new EasingDoubleKeyFrame(delta, KeyTime.FromPercent(0.25)),
                        new EasingDoubleKeyFrame(0, KeyTime.FromPercent(0.5)),
                        new EasingDoubleKeyFrame(-delta, KeyTime.FromPercent(0.75)),
                        new EasingDoubleKeyFrame(0, KeyTime.FromPercent(1))
                    }
                };

                transform.BeginAnimation(TranslateTransform.XProperty, animation);
            });
        }

        public static bool SaveToPickedFile(this BitmapSource bitmap, string defaultFileName = null)
        {
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".png",
                Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp|TIFF Image|*.tiff"
            };

            if (defaultFileName != null)
            {
                sfd.FileName = Path.GetFileNameWithoutExtension(defaultFileName);

                var dir = Path.GetDirectoryName(defaultFileName);

                if (dir != null)
                {
                    sfd.InitialDirectory = dir;
                }
            }
            else sfd.FileName = "Untitled";

            if (!sfd.ShowDialog().GetValueOrDefault())
                return false;

            BitmapEncoder encoder;

            // Filter Index starts from 1
            switch (sfd.FilterIndex)
            {
                case 2:
                    encoder = new JpegBitmapEncoder();
                    break;

                case 3:
                    encoder = new BmpBitmapEncoder();
                    break;

                case 4:
                    encoder = new TiffBitmapEncoder();
                    break;

                default:
                    encoder = new PngBitmapEncoder();
                    break;
            }

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = sfd.OpenFile())
            {
                encoder.Save(stream);
            }

            return true;
        }

        public static void Bind(this FrameworkElement control, DependencyProperty dependencyProperty, IReactiveProperty property)
        {
            control.SetBinding(dependencyProperty,
                new Binding(nameof(property.Value))
                {
                    Source = property,
                    Mode = BindingMode.TwoWay
                });
        }

        public static void BindOne<T>(this FrameworkElement control, DependencyProperty dependencyProperty, IReadOnlyReactiveProperty<T> property)
        {
            control.SetBinding(dependencyProperty,
                new Binding(nameof(property.Value))
                {
                    Source = property,
                    Mode = BindingMode.OneWay
                });
        }
    }
}