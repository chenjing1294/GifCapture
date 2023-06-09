﻿using System;
using System.Drawing;

namespace GifCapture.Gif
{
    /// <summary>
    /// Provides images.
    /// Must provide in 32-bpp RGB.
    /// </summary>
    public interface IImageProvider : IDisposable
    {
        /// <summary>
        /// Capture an image.
        /// </summary>
        Bitmap Capture();

        /// <summary>
        /// Height of Captured image.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Width of Captured image.
        /// </summary>
        int Width { get; }
    }
}