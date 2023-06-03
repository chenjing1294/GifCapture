using System;
using System.Runtime.InteropServices;

namespace GifCapture.Native.Structs
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int dimension)
        {
            Left = Top = Right = Bottom = dimension;
        }
    }
}