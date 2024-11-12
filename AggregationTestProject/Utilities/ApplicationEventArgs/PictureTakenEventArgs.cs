using AggregationTestProject.Services.Utilities;
using System.Drawing;

namespace AggregationTestProject.Utilities.ApplicationEventArgs
{
    public class PictureTakenEventArgs : EventArgs
    {
        public Bitmap Bitmap { get; set; }
        public Bitmap OverlayedBitmap
        {
            get
            {
                if (Bitmap is null)
                {
                    return null;
                }

                var overlayedBitmap = BitmapEditor.OverlayPolygonsOnImage(Bitmap, ReadCodesPositions);

                return overlayedBitmap;
            }
        }

        public Bitmap CroppedBitmap
        {
            get
            {
                if (Bitmap is null)
                {
                    return null;
                }

                return BitmapEditor.CropBitmap(OverlayedBitmap, 3);
            }
        }
        public List<string> CodesAsList { get; set; }
        public string CodesAsString { get; set; }
        public string ReadCodesPositions { get; }

        public PictureTakenEventArgs(Bitmap bitmap, List<string> readCodes, string codesAsString, string readCodesPositions)
        {
            Bitmap = bitmap;
            CodesAsList = readCodes;
            CodesAsString = codesAsString;
            ReadCodesPositions = readCodesPositions;
        }
    }
}
