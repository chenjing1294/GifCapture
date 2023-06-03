using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GifCapture.Base;
using GifCapture.Images;
using GifCapture.Models;
using GifCapture.Screen;

namespace GifCapture.ViewModels
{
    public class ScreenPickerViewModel
    {
        public ScreenPickerViewModel(IScreen screen, double scale)
        {
            this.Screen = screen;
            using (IBitmapImage bmp = ScreenShot.Capture(screen.Rectangle))
            {
                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    PngBitmapDecoder decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    Image = new ImageBrush(decoder.Frames[0]);

                    Left = (screen.Rectangle.Left / Dpi.X - SystemParameters.VirtualScreenLeft) * scale;
                    Top = (screen.Rectangle.Top / Dpi.Y - SystemParameters.VirtualScreenTop) * scale;
                    Width = screen.Rectangle.Width / Dpi.X * scale;
                    Height = screen.Rectangle.Height / Dpi.Y * scale;
                }
            }
        }

        public double Left { get; }
        public double Top { get; }

        public double Width { get; }
        public double Height { get; }

        public IScreen Screen { get; }

        public Brush Image { get; }
    }
}