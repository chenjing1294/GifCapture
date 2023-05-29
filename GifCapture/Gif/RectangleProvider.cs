using System;
using System.Drawing;
using GifCapture.Native;

namespace GifCapture.Gif
{
    public class RectangleProvider : IImageProvider
    {
        readonly Rectangle _rectangle;
        readonly bool _includeCursor;

        readonly IntPtr _hdcSrc;
        readonly ITargetDeviceContext _dcTarget;

        public RectangleProvider(Rectangle rectangle, bool includeCursor = false)
        {
            _rectangle = rectangle;
            _includeCursor = includeCursor;
            Width = rectangle.Size.Width;
            Height = rectangle.Size.Height;

            _hdcSrc = User32.GetDC(IntPtr.Zero);
            _dcTarget = new GdiTargetDeviceContext(_hdcSrc, Width, Height);
        }

        private void OnCapture()
        {
            Rectangle rect = _rectangle;
            IntPtr hdcDest = _dcTarget.GetDc();

            Gdi32.StretchBlt(hdcDest, 0, 0, Width, Height,
                _hdcSrc, rect.X, rect.Y, Width, Height,
                (int) CopyPixelOperation.SourceCopy);

            if (_includeCursor)
            {
                //TODO
            }
        }

        public Bitmap Capture()
        {
            OnCapture();
            Bitmap img = _dcTarget.GetBitmap();
            return img;
        }

        /// <summary>
        /// Height of Captured image.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Width of Captured image.
        /// </summary>
        public int Width { get; }

        public void Dispose()
        {
            _dcTarget.Dispose();
            User32.ReleaseDC(IntPtr.Zero, _hdcSrc);
        }
    }
}