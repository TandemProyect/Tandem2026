using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Desing
{
    public class ImageHelper
    {

        public enum Dimensions
        {
            Width,
            Height
        }

        public enum AnchorPosition
        {
            Top = 0,
            Center = 1,
            Bottom = 2
        }

        public static void RedimensionarImagen(string rutaOrigen, string nombreArchivo, int width, int height, AnchorPosition ap)
        {
            Image imgPhotoVert = Image.FromFile(rutaOrigen + nombreArchivo);
            Image imgPhoto = null;

            imgPhoto = Crop(imgPhotoVert, width, height, ap);
            imgPhoto.Save(rutaOrigen + "_" + nombreArchivo, GetImageFormat(imgPhotoVert));
            imgPhoto.Dispose();
        }

        private static Image Crop(Image imgPhoto, int Width, int Height, AnchorPosition Anchor)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                switch (Anchor)
                {
                    case AnchorPosition.Top:
                        destY = 0;
                        break;
                    case AnchorPosition.Bottom:
                        destY = (int)(Height - (sourceHeight * nPercent));
                        break;
                    default:
                        destY = (int)((Height - (sourceHeight * nPercent)) / 2);
                        break;
                }
            }
            else
            {
                nPercent = nPercentH;
                switch (Anchor)
                {
                    //case AnchorPosition.Left:
                    //    destX = 0;
                    //    break;
                    //case AnchorPosition.Right:
                    //    destX = (int)(Width - (sourceWidth * nPercent));
                    //    break;
                    default:
                        destX = (int)((Width - (sourceWidth * nPercent)) / 2);
                        break;
                }
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        private static ImageFormat GetImageFormat(System.Drawing.Image img)
        {
            if (img.RawFormat.Equals(ImageFormat.Jpeg))
                return ImageFormat.Jpeg;
            if (img.RawFormat.Equals(ImageFormat.Bmp))
                return ImageFormat.Bmp;
            if (img.RawFormat.Equals(ImageFormat.Png))
                return ImageFormat.Png;
            if (img.RawFormat.Equals(ImageFormat.Emf))
                return ImageFormat.Emf;
            if (img.RawFormat.Equals(ImageFormat.Exif))
                return ImageFormat.Exif;
            if (img.RawFormat.Equals(ImageFormat.Gif))
                return ImageFormat.Gif;
            if (img.RawFormat.Equals(ImageFormat.Icon))
                return ImageFormat.Icon;
            if (img.RawFormat.Equals(ImageFormat.MemoryBmp))
                return ImageFormat.MemoryBmp;
            if (img.RawFormat.Equals(ImageFormat.Tiff))
                return ImageFormat.Tiff;
            else
                return ImageFormat.Wmf;
        }
    }
}