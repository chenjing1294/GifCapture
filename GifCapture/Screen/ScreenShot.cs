using System.Drawing;
using GifCapture.Base;
using GifCapture.Images;
using GifCapture.Services;

namespace GifCapture.Screen
{
    /// <summary>
    /// Contains methods for taking ScreenShots
    /// </summary>
    public static class ScreenShot
    {
        /// <summary>
        /// Captures the entire Desktop.
        /// </summary>
        /// <param name="includeCursor">Whether to include the Mouse Cursor.</param>
        /// <returns>The Captured Image.</returns>
        public static IBitmapImage Capture(bool includeCursor = false)
        {
            var platformServices = ServiceProvider.IPlatformServices;
            return Capture(platformServices.DesktopRectangle, includeCursor);
        }

        /// <summary>
        /// Capture transparent Screenshot of a Window.
        /// </summary>
        /// <param name="window">The <see cref="IWindow"/> to Capture.</param>
        /// <param name="includeCursor">Whether to include Mouse Cursor.</param>
        public static IBitmapImage CaptureTransparent(IWindow window, bool includeCursor = false)
        {
            var platformServices = ServiceProvider.IPlatformServices;
            return platformServices.CaptureTransparent(window, includeCursor);
        }

        /// <summary>
        /// Captures a Specific Region.
        /// </summary>
        /// <param name="region">A <see cref="Rectangle"/> specifying the Region to Capture.</param>
        /// <param name="includeCursor">Whether to include the Mouse Cursor.</param>
        /// <returns>The Captured Image.</returns>
        public static IBitmapImage Capture(Rectangle region, bool includeCursor = false)
        {
            var platformServices = ServiceProvider.IPlatformServices;
            return platformServices.Capture(region, includeCursor);
        }
    }
}