using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;


namespace PuzzleGameLibrary.Helpers {
    public static class ImageStreamHelper {

        /// <summary>
        /// Get resized size with preservation of aspect ratio.
        /// Source: http://stackoverflow.com/questions/1940581/c-sharp-image-resizing-to-different-size-while-preserving-aspect-ratio
        /// </summary>
        /// <param name="CurrentDimensions">Size to resize with preservation of aspect ratio</param>
        /// <param name="maxWidth">Max width of new size</param>
        /// <param name="maxHeight">Max height of new size</param>
        /// <returns>
        /// Resized size (possibly unchanged) 
        /// with preservation of original size aspect ratio 
        /// </returns>
        public static System.Drawing.Size ResizeKeepAspect(System.Drawing.Size CurrentDimensions, int maxWidth, int maxHeight) {
            int newHeight = CurrentDimensions.Height;
            int newWidth = CurrentDimensions.Width;

            if (maxWidth > 0 && newWidth > maxWidth) {
                Decimal divider = Math.Abs((Decimal)newWidth / (Decimal)maxWidth);
                newWidth = maxWidth;
                newHeight = (int)Math.Round((Decimal)(newHeight / divider));
            }

            if (maxHeight > 0 && newHeight > maxHeight) {
                Decimal divider = Math.Abs((Decimal)newHeight / (Decimal)maxHeight);
                newHeight = maxHeight;
                newWidth = (int)Math.Round((Decimal)(newWidth / divider));
            }

            return new System.Drawing.Size(newWidth, newHeight);
        }

        /// <summary>
        /// Crop Bitmap to a 1:1 aspect ratio.
        /// The cropped area is centered int the uncropped.
        /// </summary>
        /// <param name="imgToResize">Bitmap to resize</param>
        /// <returns>Cropped Bitmap at 1:1 aspect ratio</returns>
        public static Bitmap ResizeImageToSquare(Bitmap imgToResize) {
            int w = imgToResize.Width;
            int h = imgToResize.Height;

            w = Math.Min(w, h);
            h = Math.Min(w, h);

            int paddingHor = (imgToResize.Width - w) / 2;
            int paddingVert = (imgToResize.Height - h) / 2;

            try {
                Bitmap b = new Bitmap(w, h);
                using (Graphics g = Graphics.FromImage((Image)b)) {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, -paddingHor, -paddingVert, imgToResize.Width, imgToResize.Height);
                }
                return b;
            } catch { }

            return null;
        }

        /// <summary>
        /// Resize Bitmap to given size
        /// </summary>
        /// <param name="imgToResize">Bitmap to resize</param>
        /// <param name="size">Size to resize bitmap to</param>
        /// <returns>Resized Bitmap</returns>
        public static Bitmap ResizeImage(Bitmap imgToResize, System.Drawing.Size size) {
            try {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((Image)b)) {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }
                return b;
            } catch { }

            return null;
        }

        // 
        /// <summary>
        /// Convert System.IO.Stream to Byte array.
        /// Source: http://stackoverflow.com/questions/1080442/how-to-convert-an-stream-into-a-byte-in-c
        /// </summary>
        /// <param name="stream">Stream to convert</param>
        /// <returns>Byte array from stream</returns>
        public static Byte[] ToByteArray(this Stream stream) {
            Int32 length = stream.Length > Int32.MaxValue ? Int32.MaxValue : Convert.ToInt32(stream.Length);
            Byte[] buffer = new Byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// Convert System.Drawing.Image to byte array
        /// </summary>
        /// <param name="img">Image to convert</param>
        /// <returns>Byte array from Image</returns>
        public static byte[] ImageToBytes(Image img) {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Convert System.Drawing.Bitmap to BitmapSource (for use in WPF).
        /// Source: // http://stackoverflow.com/questions/12368626/bitmap-class-in-wpf
        /// </summary>
        /// <param name="source">Bitmap source</param>
        /// <returns>BitmapSource from Bitmap</returns>
        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            IntPtr ip = source.GetHbitmap();
            try {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                    IntPtr.Zero, Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            } finally {
                DeleteObject(ip);
            }
        }

        /// <summary>
        /// Convert Byte array to BitmapImage
        /// </summary>
        /// <param name="bytes">Byte array to convert</param>
        /// <returns>BitmapImage from Byte array</returns>
        public static BitmapImage BitmapImageFromByteArray(Byte[] bytes) {
            MemoryStream stream = new MemoryStream(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        /// <summary>
        /// Convert Byte array to ImageSource
        /// </summary>
        /// <param name="bytes">Byte array to convert</param>
        /// <returns>ImageSource from Byte array</returns>
        public static ImageSource ImageSourceFromByteArray(Byte[] bytes) {
            MemoryStream stream = new MemoryStream(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }
 
        /// <summary>
        /// Convert ImageSource to System.Drawing.Image
        /// Source: http://stackoverflow.com/questions/1201518/convert-system-windows-media-imagesource-to-system-drawing-bitmap
        /// </summary>
        /// <param name="image">ImageSource to convert</param>
        /// <returns>Image from ImageStream</returns>
        public static System.Drawing.Image ImageWpfToGDI(System.Windows.Media.ImageSource image) {
            MemoryStream ms = new MemoryStream();
            var encoder = new System.Windows.Media.Imaging.BmpBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image as System.Windows.Media.Imaging.BitmapSource));
            encoder.Save(ms);
            ms.Flush();
            return System.Drawing.Image.FromStream(ms);
        }

    }

}
