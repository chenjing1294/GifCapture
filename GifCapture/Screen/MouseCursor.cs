using System;
using System.Drawing;
using System.Runtime.InteropServices;
using GifCapture.Native;
using GifCapture.Native.Enums;
using GifCapture.Native.Structs;

namespace GifCapture.Screen
{
    /// <summary>
    /// Draws the MouseCursor on an Image
    /// </summary>
    static class MouseCursor
    {
        const int CursorShowing = 1;

        /// <summary>
        /// Draws this overlay.
        /// </summary>
        /// <param name="g">A <see cref="Graphics"/> object to draw upon.</param>
        /// <param name="transform">Point Transform Function.</param>
        public static void Draw(Graphics g, Func<Point, Point> transform = null)
        {
            var hIcon = GetIcon(transform, out var location);

            if (hIcon == IntPtr.Zero)
                return;

            var bmp = Icon.FromHandle(hIcon).ToBitmap();
            User32.DestroyIcon(hIcon);

            try
            {
                using (bmp)
                {
                    SolidBrush solidBrush = new SolidBrush(Color.FromArgb(200, 191, 222, 179));
                    Pen pen = new Pen(Color.Red);
                    int width = bmp.Size.Width + 5;
                    g.FillEllipse(solidBrush, location.X - width / 2, location.Y - width / 2, width, width);
                    g.DrawEllipse(pen, location.X - width / 2, location.Y - width / 2, width, width);
                    g.DrawImage(bmp, new Rectangle(location, bmp.Size));
                    solidBrush.Dispose();
                    pen.Dispose();
                }
            }
            catch (ArgumentException)
            {
            }
        }

        public static void Draw(IntPtr deviceContext, Func<Point, Point> transform = null)
        {
            var hIcon = GetIcon(transform, out var location);

            if (hIcon == IntPtr.Zero)
                return;

            try
            {
                User32.DrawIconEx(deviceContext,
                    location.X, location.Y,
                    hIcon,
                    0, 0, 0, IntPtr.Zero,
                    DrawIconExFlags.Normal);
            }
            finally
            {
                User32.DestroyIcon(hIcon);
            }
        }

        static IntPtr GetIcon(Func<Point, Point> transform, out Point location)
        {
            location = Point.Empty;

            var cursorInfo = new CursorInfo {cbSize = Marshal.SizeOf<CursorInfo>()};

            if (!User32.GetCursorInfo(ref cursorInfo))
                return IntPtr.Zero;

            if (cursorInfo.flags != CursorShowing)
                return IntPtr.Zero;

            var hIcon = User32.CopyIcon(cursorInfo.hCursor);

            if (hIcon == IntPtr.Zero)
                return IntPtr.Zero;

            if (!User32.GetIconInfo(hIcon, out var icInfo))
                return IntPtr.Zero;

            var hotspot = new Point(icInfo.xHotspot, icInfo.yHotspot);

            location = new Point(cursorInfo.ptScreenPos.X - hotspot.X, cursorInfo.ptScreenPos.Y - hotspot.Y);

            if (transform != null)
                location = transform(location);

            Gdi32.DeleteObject(icInfo.hbmColor);
            Gdi32.DeleteObject(icInfo.hbmMask);

            return hIcon;
        }
    }
}