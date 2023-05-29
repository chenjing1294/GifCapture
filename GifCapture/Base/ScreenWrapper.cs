using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GifCapture.Base
{
    class ScreenWrapper : IScreen
    {
        readonly System.Windows.Forms.Screen _screen;

        ScreenWrapper(System.Windows.Forms.Screen screen)
        {
            _screen = screen;
        }

        public Rectangle Rectangle => _screen.Bounds;

        public string DeviceName => _screen.DeviceName;

        public static IEnumerable<IScreen> Enumerate() => System.Windows.Forms.Screen.AllScreens.Select(m => new ScreenWrapper(m));
    }
}