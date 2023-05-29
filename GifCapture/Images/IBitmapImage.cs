using System;
using System.Drawing.Imaging;
using System.IO;

namespace GifCapture.Images
{
    public interface IBitmapImage : IDisposable
    {
        int Width { get; }

        int Height { get; }

        void Save(string fileName, ImageFormat format);

        void Save(Stream stream, ImageFormat format);
    }
}