using System.Drawing;
using GifCapture.Native.Structs;

namespace GifCapture.Native
{
    static class NativeExtensions
    {
        public static Rectangle ToRectangle(this RECT r) => Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
    }
}