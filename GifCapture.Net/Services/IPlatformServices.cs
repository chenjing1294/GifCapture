using System.Collections.Generic;
using System.Drawing;
using GifCapture.Base;
using GifCapture.Images;

namespace GifCapture.Services
{
    public interface IPlatformServices
    {
        IEnumerable<IScreen> EnumerateScreens();
        IEnumerable<IWindow> EnumerateWindows();
        IEnumerable<IWindow> EnumerateAllWindows();
        IWindow DesktopWindow { get; }

        Rectangle DesktopRectangle { get; }

        IBitmapImage CaptureTransparent(IWindow window, bool includeCursor = false);

        Point CursorPosition { get; }

        IBitmapImage Capture(Rectangle region, bool includeCursor = false);
    }
}