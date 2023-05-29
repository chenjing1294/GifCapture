using System;
using System.Drawing;
using System.Runtime.InteropServices;


namespace GifCapture.Native.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CursorInfo
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public Point ptScreenPos;
    }
}