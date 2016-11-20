using PuzzleGameLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PuzzleGameLibrary.Model {

    /// <summary>
    /// A custom Bitmap class for fast pixel manipulation
    /// </summary>
    public partial class ARGBBitmapArray {

        // Byte size of 1 pixel
        private int byteSize;

        private int junkBytesOnEachLine;

        private BitmapData bmpData;
        private IntPtr ptr;

        private Bitmap bitmap;

        private ARGBPixel[][] pixels;

        private int _width, _height;

        /// <summary>
        /// Width of internal image
        /// </summary>
        public int Width {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Height of internal image
        /// </summary>
        public int Height {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Initialize ARGBBitmapArray object
        /// </summary>
        /// <param name="bitmap">Bitmap to manipulate</param>
        public ARGBBitmapArray(Bitmap bitmap) {
            this.bitmap = bitmap;
            _width = bitmap.Width;
            _height = bitmap.Height;
            this.pixels = new ARGBPixel[Height][];

            CalculateSizeParameters();

            // Lock the bits
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            bmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);

            // Get the address of the first line
            ptr = bmpData.Scan0;

            DecodeBytes();

        }

        private void CalculateSizeParameters() {

            int bitSize = 24;

            if (Bitmap.IsAlphaPixelFormat(bitmap.PixelFormat)) {

                bitSize = 32;

            }

            byteSize = bitSize / 8;

            int junkBitsOnEachLine = 32 - (int)((Width * bitSize) % 32);
            if (junkBitsOnEachLine == 32) {
                junkBytesOnEachLine = 0;

            } else {
                junkBytesOnEachLine = junkBitsOnEachLine / 8;

            }

        }

        // Junk bytes occur when we have pixel lines that are not divisable by 32bit
        // Windows optimizes bitmaps to always start a new line in memory on a new 32-bit boundary
        // RGB pixel = 24 bits
        // 24 24 24 24 24 ==> 120 % 32 = 24 junk bits 
        // (will give 3 black pixel junk bytes ==> 0 0 0)
        //
        // when 32 bits per pixel are used, you will never encounter junk pixels
        private void RemoveJunkBytes(ref byte[] rgbValues) {

            int bytes = BytesSizeForPixels(false);

            byte[] rgbValuesWithoutJunk = new byte[bytes];

            int lineWidth = (int)(Width * byteSize);

            int i = 1;
            int j = -1;
            int bytesToSkip = 0;

            foreach (byte b in rgbValues) {

                if (bytesToSkip > 0) {
                    bytesToSkip--;
                    continue;
                }

                j++;
                rgbValuesWithoutJunk[j] = b; 

                if (i >= lineWidth) {

                    i = 1;
                    bytesToSkip = junkBytesOnEachLine;
                    continue;
                }

                i++;

            }

            rgbValues = rgbValuesWithoutJunk;

        }

        private int BytesSizeForPixels(bool includingJunk) {

            int size = (int)(Width * Height * byteSize);

            if (includingJunk) {
                size += (int) (junkBytesOnEachLine * Height);
            }

            return size;
        }

        private void DecodeBytes() {

            int bytes = BytesSizeForPixels(true);

            byte[] rgbValues = new byte[bytes];

            // Copy the (A)RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            RemoveJunkBytes(ref rgbValues);

            for (int line = 0; line < this.Height; line++) {

                for (int i = 0; i < this.Width; i++) {

                    int lineOffset = (int)(line * Width * byteSize);

                    byte alpha = (byteSize == 4) ? (byte)rgbValues[lineOffset + i * byteSize + 3] : (byte)255;

                    ARGBPixel pixel = new ARGBPixel(alpha,
                                                    rgbValues[lineOffset + i * byteSize + 2],
                                                    rgbValues[lineOffset + i * byteSize + 1],
                                                    rgbValues[lineOffset + i * byteSize]);

                    if (pixels[line] == null) {
                        pixels[line] = new ARGBPixel[Width];
                    }

                    pixels[line][i] = pixel;

                }

            }

        }

        private void EncodeBytes() {

            int bytes = BytesSizeForPixels(true);
            byte[] rgbValuesNew = new byte[bytes];

            int rgbIndex = 0;
            for (int line = 0; line < this.Height; line++) {

                for (int i = 0; i < this.Width; i++) {

                    ARGBPixel pixel = pixels[line][i];

                    rgbValuesNew[rgbIndex + 2] = (byte)pixel.Red;
                    rgbValuesNew[rgbIndex + 1] = (byte)pixel.Green;
                    rgbValuesNew[rgbIndex] = (byte)pixel.Blue;

                    // alpha channel
                    if (byteSize == 4) {
                        rgbValuesNew[rgbIndex + 3] = (byte)pixel.Alpha;
                    }

                    if (i == Width - 1) {
                        // end of line
                        // add junk pixel(s)

                        rgbIndex += junkBytesOnEachLine;

                    }

                    rgbIndex += byteSize;
                }

            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValuesNew, 0, ptr, bytes);
            
        }

        /// <summary>
        /// Clean up the use of the bitmap bits
        /// </summary>
        public void CleanUp() {
            // Unlock the bits
            bitmap.UnlockBits(bmpData);

        }

        private ARGBPixel[][] GetPixelsCopy() {

            ARGBPixel[][] pixelsCopy = new ARGBPixel[this.Height][];

            for (int line = 0; line < this.Height; line++) {

                if (pixelsCopy[line] == null) {
                    pixelsCopy[line] = new ARGBPixel[Width];
                }

                Array.Copy(pixels[line], pixelsCopy[line], Width);

                for (int i = 0; i < this.Width; i++) {

                    ARGBPixel pixel = pixels[line][i];

                    ARGBPixel copiedPixel = new ARGBPixel(pixel.Alpha, pixel.Red, pixel.Green, pixel.Blue);

                    pixelsCopy[line][i] = copiedPixel;

                }

            }

            return pixelsCopy;
        }

        /// <summary>
        /// Crop image.
        /// Important: only works for shrinking!
        /// </summary>
        /// <param name="height">New height</param>
        /// <param name="width">New Width</param>
        public void Crop(int height, int width) {

            ARGBPixel[][] newPixels = new ARGBPixel[height][];

            for (int line = 0; line < height; line++) {

                if (newPixels[line] == null) {
                    newPixels[line] = new ARGBPixel[width];
                }

                for (int i = 0; i < width; i++) {

                    if (line >= this.Height ||
                        i >= this.Width) {

                        newPixels[line][i] = new ARGBPixel();

                        continue;

                    }

                    newPixels[line][i] = pixels[line][i];

                }

            }

            _width = width;
            _height = height;

            pixels = newPixels;

            CalculateSizeParameters();

            Rectangle rect = new Rectangle(0, 0, (int)Width, (int)Height);

            bitmap = bitmap.Clone(rect, bitmap.PixelFormat);

            // Lock the bits
            bmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);

            // Get the address of the first line
            ptr = bmpData.Scan0;

        }

        /// <summary>
        /// Swap 2 pixels
        /// </summary>
        /// <param name="line1">Line of pixel 1</param>
        /// <param name="i1">Row of pixel 1</param>
        /// <param name="line2">Line of pixel 2</param>
        /// <param name="i2">Row of pixel 2</param>
        public void SwapPixels(int line1, int i1, int line2, int i2) {

            ARGBPixel a = pixels[line1][i1];
            ARGBPixel b = pixels[line2][i2];

            pixels[line1][i1] = b;
            pixels[line2][i2] = a;

        }

        /// <summary>
        /// Move a pixel.
        /// (destination pixel becomes white)
        /// </summary>
        /// <param name="line1">Line of pixel 1</param>
        /// <param name="i1">Row of pixel 1</param>
        /// <param name="line2">Line of pixel 2</param>
        /// <param name="i2">Row of pixel 2</param>
        public void MovePixel(int line1, int i1, int line2, int i2) {

            ARGBPixel a = pixels[line1][i1];
            ARGBPixel b = pixels[line2][i2];

            pixels[line1][i1] = new ARGBPixel(255, 255, 255, 255);
            pixels[line2][i2] = a;

        }

        /// <summary>
        /// Make a pixel tranparant.
        /// (and white in case no alpha channel is processed)
        /// </summary>
        /// <param name="line">Line of pixel</param>
        /// <param name="i">Row of pixel</param>
        public void MakePixelTransparent(int line, int i) {

            ARGBPixel pixel = pixels[line][i];
            pixel.Red = 255;
            pixel.Green = 255;
            pixel.Blue = 255;
            pixel.Alpha = 0;

        }

        /// <summary>
        /// Make a pixel white.
        /// </summary>
        /// <param name="line">Line of pixel</param>
        /// <param name="i">Row of pixel</param>
        public void MakePixelWhite(int line, int i) {

            ARGBPixel pixel = pixels[line][i];
            pixel.Red = 255;
            pixel.Green = 255;
            pixel.Blue = 255;

        }

        /// <summary>
        /// Set a pixel color
        /// </summary>
        /// <param name="line">Line of pixel</param>
        /// <param name="i">Row of pixel</param>
        /// <param name="color">Color to set</param>
        public void SetPixel(int line, int i, ARGBPixel color) {

            ARGBPixel pixel = pixels[line][i];
            pixel.Red = color.Red;
            pixel.Green = color.Green;
            pixel.Blue = color.Blue;
            pixel.Alpha = color.Alpha;

        }

        /// <summary>
        /// Fill rectangle with a color
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="color">Color to set</param>
        public void FillRect(Rect rect, ARGBPixel color) {

            for (int i = (int)rect.X; i < rect.X + rect.Width; i++) {

                for (int j = (int)rect.Y; j < rect.Y + rect.Height; j++) {

                    if (i < 0 ||
                        j < 0 ||
                        i >= _width ||
                        j >= _height) {
                        continue;
                    }

                    ARGBPixel pixel = pixels[j][i];
                    pixel.Alpha = color.Alpha;
                    pixel.Red = color.Red;
                    pixel.Green = color.Green;
                    pixel.Blue = color.Blue;

                }

            }

        }

        /// <summary>
        /// Get Stream of current image
        /// </summary>
        /// <returns></returns>
        public Stream GetBitmapStream() {
            EncodeBytes();
            return new MemoryStream(ImageStreamHelper.ImageToBytes(bitmap));
        }

        /// <summary>
        /// Get BitmapImage of current image
        /// </summary>
        /// <returns></returns>
        public BitmapImage GetBitmapImage() {
            EncodeBytes();
            return ImageStreamHelper.BitmapImageFromByteArray(ImageStreamHelper.ImageToBytes(bitmap));
        }
        
    }
}
