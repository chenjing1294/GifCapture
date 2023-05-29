using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using GifCapture.Base;
using GifCapture.Images;
using GifCapture.Native;
using GifCapture.Screen;

namespace GifCapture.Services
{
    public class WindowsPlatformServices : IPlatformServices
    {
        public IEnumerable<IScreen> EnumerateScreens()
        {
            return ScreenWrapper.Enumerate();
        }

        public IEnumerable<IWindow> EnumerateWindows()
        {
            return Window.EnumerateVisible();
        }

        public IEnumerable<IWindow> EnumerateAllWindows()
        {
            return Window
                .Enumerate()
                .Where(w => w.IsVisible)
                .SelectMany(GetAllChildren);
        }

        public IWindow DesktopWindow => Window.DesktopWindow;

        public Rectangle DesktopRectangle => SystemInformation.VirtualScreen;

        public IBitmapImage CaptureTransparent(IWindow window, bool includeCursor = false)
        {
            return ScreenShotInternal.CaptureTransparent(window, includeCursor, this);
        }

        public Point CursorPosition
        {
            get
            {
                var p = new Point();
                User32.GetCursorPos(ref p);
                return p;
            }
        }

        public IBitmapImage Capture(Rectangle region, bool includeCursor = false)
        {
            return ScreenShotInternal.Capture(region, includeCursor);
        }

        IEnumerable<Window> GetAllChildren(Window window)
        {
            var children = window
                .EnumerateChildren()
                .Where(w => w.IsVisible);

            foreach (var child in children)
            {
                foreach (var grandchild in GetAllChildren(child))
                {
                    yield return grandchild;
                }
            }

            yield return window;
        }
    }
}