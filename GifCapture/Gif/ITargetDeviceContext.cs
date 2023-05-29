using System;
using System.Drawing;

namespace GifCapture.Gif
{
    public interface ITargetDeviceContext : IDisposable
    {
        IntPtr GetDc();

        Bitmap GetBitmap();
    }
}