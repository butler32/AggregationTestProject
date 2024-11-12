using AggregationTestProject.Constants;

namespace AggregationTestProject.Services.Utilities
{
    public static class ZPLCalculator
    {
        public static (double X, double Y) CalculateTopLeftCorner(
          double dataMatrixWidthDots,
          double dataMatrixHeightDots,
          double labelWidthDots,
          double labelHeightDots)
        {
            // Вычисление смещений для центрирования DataMatrix на этикетке
            double offsetX = (labelWidthDots - dataMatrixWidthDots) / 2;
            double offsetY = (labelHeightDots - dataMatrixHeightDots) / 2;

            return (offsetX, offsetY);
        }

        public static double DotsToMillimeters(double dots, DPI dpi)
        {
            double mm;

            switch (dpi)
            {
                case DPI.DPI203:
                    mm = dots / 8.0; // 1mm = 8 dots for 203 DPI
                    break;
                case DPI.DPI300:
                    mm = dots / 11.8; // 1mm = 11.8 dots for 300 DPI
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dpi), dpi, null);
            }

            return Math.Round(mm, 2); // Return value rounded to 2 decimal places
        }

        public static double MillimetersToDots(double millimeters, DPI dpi)
        {
            double dots;

            switch (dpi)
            {
                case DPI.DPI203:
                    dots = millimeters * 8.0; // 1mm = 8 dots for 203 DPI
                    break;
                case DPI.DPI300:
                    dots = millimeters * 11.8; // 1mm = 11.8 dots for 300 DPI
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dpi), dpi, null);
            }

            return Math.Round(dots, 2); // Return value rounded to 2 decimal places
        }
    }
}
