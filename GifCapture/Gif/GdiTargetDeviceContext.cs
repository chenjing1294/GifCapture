using System;
using System.Drawing;
using GifCapture.Native;

namespace GifCapture.Gif
{
    public class GdiTargetDeviceContext : ITargetDeviceContext
    {
        readonly IntPtr _hdcDest, _hBitmap;

        public GdiTargetDeviceContext(IntPtr srcDc, int width, int height)
        {
            _hdcDest = Gdi32.CreateCompatibleDC(srcDc);
            _hBitmap = Gdi32.CreateCompatibleBitmap(srcDc, width, height);
            Gdi32.SelectObject(_hdcDest, _hBitmap);
        }

        public void Dispose()
        {
            Gdi32.DeleteDC(_hdcDest);
            Gdi32.DeleteObject(_hBitmap);
        }

        public IntPtr GetDc() => _hdcDest;

        public Bitmap GetBitmap()
        {
            return Image.FromHbitmap(_hBitmap);
        }
    }
}