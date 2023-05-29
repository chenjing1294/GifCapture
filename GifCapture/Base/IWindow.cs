using System;
using System.Drawing;

namespace GifCapture.Base
{
    /// <summary>
    /// Minimal representation of a Window.
    /// </summary>
    public interface IWindow
    {
        bool IsAlive { get; }

        bool IsVisible { get; }

        bool IsMaximized { get; }

        IntPtr Handle { get; }

        string Title { get; }

        Rectangle Rectangle { get; }
    }
}