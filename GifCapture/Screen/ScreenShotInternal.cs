using System;
using System.Drawing;
using GifCapture.Base;
using GifCapture.Images;
using GifCapture.Services;

namespace GifCapture.Screen
{
    /// <summary>
    /// Contains methods for taking ScreenShots
    /// </summary>
    static class ScreenShotInternal
    {
        /// <summary>
        /// Capture transparent Screenshot of a Window.
        /// </summary>
        public static IBitmapImage CaptureTransparent(IWindow window, bool includeCursor, IPlatformServices platformServices)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            var backdrop = new WindowScreenShotBackdrop(window, platformServices);

            backdrop.ShowWhite();

            var r = backdrop.Rectangle;

            // Capture screenshot with white background
            using (var whiteShot = CaptureInternal(r))
            {
                backdrop.ShowBlack();

                // Capture screenshot with black background
                using (var blackShot = CaptureInternal(r))
                {
                    backdrop.Dispose();

                    var transparentImage = GraphicsExtensions.DifferentiateAlpha(whiteShot, blackShot);

                    if (transparentImage == null)
                        return null;

                    // Include Cursor only if within window
                    if (includeCursor && r.Contains(platformServices.CursorPosition))
                    {
                        using (var g = Graphics.FromImage(transparentImage))
                        {
                            MouseCursor.Draw(g, P => new Point(P.X - r.X, P.Y - r.Y));
                        }
                    }

                    return new DrawingImage(transparentImage.CropEmptyEdges());
                }
            }
        }

        static Bitmap CaptureInternal(Rectangle region, bool includeCursor = false)
        {
            var bmp = new Bitmap(region.Width, region.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(region.Location, Point.Empty, region.Size, CopyPixelOperation.SourceCopy);
                if (includeCursor)
                {
                    MouseCursor.Draw(g, p => new Point(p.X - region.X, p.Y - region.Y));
                }

                g.Flush();
            }

            return bmp;
        }

        /// <summary>
        /// Captures a Specific Region.
        /// </summary>
        /// <param name="region">A <see cref="Rectangle"/> specifying the Region to Capture.</param>
        /// <param name="includeCursor">Whether to include the Mouse Cursor.</param>
        /// <returns>The Captured Image.</returns>
        public static IBitmapImage Capture(Rectangle region, bool includeCursor = false)
        {
            return new DrawingImage(CaptureInternal(region, includeCursor));
        }
    }
}