using Cognex.DataMan.SDK.Utils;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing.Imaging;

namespace AggregationTestProject.Services.Utilities
{
    public static class BitmapEditor
    {
        public static ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            if (bitmap is null)
            {
                return null;
            }

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public static Bitmap CropBitmap(Bitmap original, int pixelsToCrop)
        {
            if (original.Width <= 2 * pixelsToCrop || original.Height <= 2 * pixelsToCrop)
            {
                return original;
            }

            Rectangle cropRect = new(pixelsToCrop, pixelsToCrop, original.Width - 2 * pixelsToCrop, original.Height - 2 * pixelsToCrop);

            Bitmap croppedBitmap = new(cropRect.Width, cropRect.Height);

            using (Graphics graphics = Graphics.FromImage(croppedBitmap))
            {
                graphics.DrawImage(original, new Rectangle(0, 0, croppedBitmap.Width, croppedBitmap.Height), cropRect, GraphicsUnit.Pixel);
            }

            return croppedBitmap;
        }

        public static Bitmap OverlayPolygonsOnImage(Bitmap image, string svgString)
        {
            var modImage = new Bitmap(image);

            using (var graphics = Graphics.FromImage(modImage))
            {
                var resultGraphics = GraphicsResultParser.Parse(svgString, new Rectangle(0, 0, modImage.Width, modImage.Height));
                PaintResults(graphics, resultGraphics);
            }

            return modImage;
        }

        public static void PaintResults(Graphics graphics, ResultGraphics resultGraphics)
        {
            if (resultGraphics is null || resultGraphics.Polygons is null || resultGraphics.Polygons.Count <= 0)
            {
                return;
            }

            System.Drawing.Pen pen = new(resultGraphics.Polygons.First().Color);

            foreach (var polygon in resultGraphics.Polygons)
            {
                if (!pen.Color.Equals(polygon.Color))
                {
                    pen.Color = polygon.Color;
                }

                pen.Width = 10;
                graphics.DrawLines(pen, polygon.Points);
            }
        }
    }
}
