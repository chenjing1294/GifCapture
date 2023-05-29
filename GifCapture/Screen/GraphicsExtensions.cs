using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using GifCapture.Images;
using GifCapture.Native;
using GifCapture.Native.Structs;

namespace GifCapture.Screen
{
    static class GraphicsExtensions
    {
        /// <summary>
        /// Removes the Pixels on Edges matching TrimColor(default is Transparent) from the Image
        /// </summary>
        internal static unsafe Bitmap CropEmptyEdges(this Bitmap image, Color trimColor = default)
        {
            if (image == null)
                return null;

            int sizeX = image.Width,
                sizeY = image.Height;

            var r = new RECT(-1);

            using (var b = new UnsafeBitmap(image))
            {
                for (int x = 0, y = 0;;)
                {
                    var pixel = b[x, y];

                    bool Condition()
                    {
                        return trimColor.A == 0
                               && pixel->Alpha != 0
                               ||
                               trimColor.R != pixel->Red
                               && trimColor.G != pixel->Green
                               && trimColor.B != pixel->Blue;
                    }

                    if (r.Left == -1)
                    {
                        if (Condition())
                        {
                            r.Left = x;

                            x = 0;
                            y = 0;

                            continue;
                        }

                        if (y == sizeY - 1)
                        {
                            x++;
                            y = 0;
                        }
                        else y++;

                        continue;
                    }

                    if (r.Top == -1)
                    {
                        if (Condition())
                        {
                            r.Top = y;

                            x = sizeX - 1;
                            y = 0;

                            continue;
                        }

                        if (x == sizeX - 1)
                        {
                            y++;
                            x = 0;
                        }
                        else x++;

                        continue;
                    }

                    if (r.Right == -1)
                    {
                        if (Condition())
                        {
                            r.Right = x + 1;

                            x = 0;
                            y = sizeY - 1;

                            continue;
                        }

                        if (y == sizeY - 1)
                        {
                            x--;
                            y = 0;
                        }
                        else y++;

                        continue;
                    }

                    if (r.Bottom != -1)
                        continue;

                    if (Condition())
                    {
                        r.Bottom = y + 1;
                        break;
                    }

                    if (x == sizeX - 1)
                    {
                        y--;
                        x = 0;
                    }
                    else x++;
                }
            }

            if (r.Left >= r.Right || r.Top >= r.Bottom)
                return null;

            var final = image.Clone(r.ToRectangle(), image.PixelFormat);

            image.Dispose();

            return final;
        }

        /// <summary>
        /// Creates a Transparent Bitmap from a combination of a Bitmap on a White Background and another on a Black Background
        /// </summary>
        internal static unsafe Bitmap DifferentiateAlpha(Bitmap WhiteBitmap, Bitmap BlackBitmap)
        {
            if (WhiteBitmap == null || BlackBitmap == null || WhiteBitmap.Size != BlackBitmap.Size)
                return null;

            int sizeX = WhiteBitmap.Width,
                sizeY = WhiteBitmap.Height;

            var final = new Bitmap(sizeX, sizeY, PixelFormat.Format32bppArgb);

            var empty = true;

            using (var a = new UnsafeBitmap(WhiteBitmap))
            using (var b = new UnsafeBitmap(BlackBitmap))
            using (var f = new UnsafeBitmap(final))
            {
                byte ToByte(int I) => (byte) (I > 255 ? 255 : (I < 0 ? 0 : I));

                for (var y = 0; y < sizeY; ++y)
                for (var x = 0; x < sizeX; ++x)
                {
                    PixelData* pixelA = a[x, y],
                        pixelB = b[x, y],
                        pixelF = f[x, y];

                    pixelF->Alpha = ToByte((pixelB->Red - pixelA->Red + 255
                                                                      + pixelB->Green - pixelA->Green + 255
                                                                                                      + pixelB->Blue - pixelA->Blue + 255) / 3);

                    if (pixelF->Alpha > 0)
                    {
                        // Following math creates an image optimized to be displayed on a black background
                        pixelF->Red = ToByte(255 * pixelB->Red / pixelF->Alpha);
                        pixelF->Green = ToByte(255 * pixelB->Green / pixelF->Alpha);
                        pixelF->Blue = ToByte(255 * pixelB->Blue / pixelF->Alpha);

                        if (empty)
                            empty = false;
                    }
                }
            }

            return empty ? null : final;
        }

        static GraphicsPath RoundedRect(RectangleF Bounds, int Radius)
        {
            var path = new GraphicsPath();

            if (Radius == 0)
            {
                path.AddRectangle(Bounds);
                return path;
            }

            var diameter = Radius * 2;
            var arc = new RectangleF(Bounds.Location, new Size(diameter, diameter));

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = Bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = Bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = Bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF bounds, int cornerRadius)
        {
            using (var path = RoundedRect(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF bounds, int cornerRadius)
        {
            using (var path = RoundedRect(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }
    }
}