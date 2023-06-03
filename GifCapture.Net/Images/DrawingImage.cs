using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using GifCapture.Screen;

namespace GifCapture.Images
{
    public class DrawingImage : IBitmapImage
    {
        public Image Image { get; }

        public DrawingImage(Image image)
        {
            this.Image = image;
        }

        public void Dispose()
        {
            Image.Dispose();
        }

        public int Width => Image.Width;
        public int Height => Image.Height;

        public void Save(string fileName, ImageFormat format)
        {
            Image.Save(fileName, format);
        }

        public void Save(Stream stream, ImageFormat format)
        {
            Image.Save(stream, format);
        }
    }
}