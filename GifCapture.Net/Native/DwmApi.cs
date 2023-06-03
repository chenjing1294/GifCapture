using System;
using System.Runtime.InteropServices;
using GifCapture.Native.Structs;

namespace GifCapture.Native
{
    static class DwmApi
    {
        const string DllName = "dwmapi.dll";

        [DllImport(DllName)]
        public static extern int DwmGetWindowAttribute(IntPtr Window, int Attribute, out bool Value, int Size);

        [DllImport(DllName)]
        public static extern int DwmGetWindowAttribute(IntPtr Window, int Attribute, ref RECT Value, int Size);
    }
}