using PuzzleGameLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Model {

    public partial class ARGBBitmapArray {

        /// <summary>
        /// Flip image horizontally
        /// </summary>
        public void ApplyFlipHorizontally() {

            int halfWidth = (int)(this.Width / 2);

            for (int line = 0; line < this.Height; line++) {

                for (int i = 0; i <= halfWidth; i++) {

                    int newI = -i + (int)this.Width - 1;
                    SwapPixels(line, i, line, newI);

                }

            }

        }

        /// <summary>
        /// Flip image vertically
        /// </summary>
        public void ApplyFlipVertically() {

            int halfHeight = (int)(this.Height / 2);

            for (int line = 0; line <= halfHeight; line++) {

                int newLine = -line + (int)this.Height - 1;

                for (int i = 0; i < this.Width; i++) {

                    SwapPixels(line, i, newLine, i);

                }

            }

        }

        /// <summary>
        /// Translate image Horizontally.
        /// </summary>
        /// <param name="delta">Horizontal translation</param>
        public void ApplyHorizontalTranslation(int delta) {

            ARGBPixel[][] pixelsCopy = GetPixelsCopy();

            for (int line = 0; line < this.Height; line++) {

                for (int i = (int)(this.Width) - 1 - delta; i >= 0; i--) {

                    RGBPixel pixel = pixels[line][i];

                    MovePixel(line, i, line, i + delta);

                }

            }

        }

        /// <summary>
        /// Invert image
        /// </summary>
        public void ApplyInvert() {

            for (int line = 0; line < this.Height; line++) {

                for (int i = 0; i < this.Width; i++) {

                    pixels[line][i].InvertPixel();

                }

            }

        }

        /// <summary>
        /// Make image greyscale
        /// </summary>
        public void ApplyGreyscale() {

            for (int line = 0; line < this.Height; line++) {

                for (int i = 0; i < this.Width; i++) {

                    pixels[line][i].MakeGreyscale();

                }

            }

        }

        /// <summary>
        /// Make image sepia
        /// </summary>
        public void ApplySepia() {

            for (int line = 0; line < this.Height; line++) {

                for (int i = 0; i < this.Width; i++) {

                    pixels[line][i].MakeSepia();

                }

            }

        }

        /// <summary>
        /// Apply gaussion blur using 2 motion blurs.
        /// This is exponentially faster than ApplyGaussianBlur
        /// </summary>
        public void ApplyFastGaussianBlur(int diameter) {

            if (diameter % 2 == 0) {
                throw new ArgumentException("Diameter has to be an odd integer");
            }

            GaussianBlurHelper gaussionBlurHelper = new GaussianBlurHelper(diameter);
            int radius = gaussionBlurHelper.Radius;

            double[] kernel = gaussionBlurHelper.OneDimensionalFilterKernel();

            ApplyMotionBlurHorizontally(diameter, kernel, radius);
            ApplyMotionBlurVertically(diameter, kernel, radius);

        }

        /// <summary>
        /// Apply gaussion blur.
        /// This method is very slow
        /// 
        /// Could be rewritten to OpenCL for GPU usage,
        /// which can be up to thousands of times faster.
        /// </summary>
        /// <param name="diameter"></param>
        public void ApplyGaussianBlur(int diameter) {

            if (diameter % 2 == 0) {
                throw new ArgumentException("Diameter has to be an odd integer");
            }

            ARGBPixel[][] pixelsCopy = GetPixelsCopy();

            GaussianBlurHelper gaussionBlurHelper = new GaussianBlurHelper(diameter);
            int radius = gaussionBlurHelper.Radius;

            for (int line = 0; line < this.Height; line++) {

                for (int i = 0; i < this.Width; i++) {

                    ARGBPixel pixel = pixels[line][i];

                    double red = 0;
                    double green = 0;
                    double blue = 0;
                    double alpha = 0;

                    for (int k = -radius; k <= radius; k++) {

                        for (int l = -radius; l <= radius; l++) {

                            double g = gaussionBlurHelper.Filter2DKernel[k + radius][l + radius];

                            int y = (int)Math.Min(Math.Max(line + k, 0), Height - 1);

                            int x = (int)Math.Min(Math.Max(i + l, 0), Width - 1);

                            ARGBPixel pixelToAdd = pixelsCopy[y][x];

                            red += (g * pixelToAdd.Red);
                            green += (g * pixelToAdd.Green);
                            blue += (g * pixelToAdd.Blue);
                            alpha += (g * pixelToAdd.Alpha);

                        }

                    }

                    pixel.Red = (byte)red;
                    pixel.Green = (byte)green;
                    pixel.Blue = (byte)blue;
                    pixel.Alpha = (byte)alpha;

                }

            }

        }

        /// <summary>
        /// Apply horizontal motion blur.
        /// </summary>
        /// <param name="diameter">Motion blur diameter (should be odd, not even)</param>
        public void ApplyMotionBlurHorizontally(int diameter) {

            if (diameter % 2 == 0) {
                throw new ArgumentException("Diameter has to be an odd integer");
            }

            ApplyMotionBlurHorizontally(diameter, null, 0);
        }

        /// <summary>
        /// Apply vertical motion blur.
        /// </summary>
        /// <param name="diameter">Motion blur diameter (should be odd, not even)</param>
        public void ApplyMotionBlurVertically(int diameter) {

            if (diameter % 2 == 0) {
                throw new ArgumentException("Diameter has to be an odd integer");
            }

            ApplyMotionBlurVertically(diameter, null, 0);
        }

        /// <summary>
        /// Apply horizontal motion blur.
        /// </summary>
        /// <param name="diameter">Diameter (in case kernel is null)</param>
        /// <param name="kernel">Kernel to use</param>
        /// <param name="radius">Radius of motion blur</param>
        private void ApplyMotionBlurHorizontally(int diameter, double[] kernel, int radius) {

            if (diameter % 2 == 0) {
                throw new ArgumentException("Diameter has to be an odd integer");
            }

            ARGBPixel[][] pixelsCopy = GetPixelsCopy();

            if (kernel == null) {

                GaussianBlurHelper gaussionBlurHelper = new GaussianBlurHelper(diameter);
                radius = gaussionBlurHelper.Radius;

                kernel = gaussionBlurHelper.OneDimensionalFilterKernel();

            }


            for (int line = 0; line < this.Height; line++) {

                for (int i = 0; i < this.Width; i++) {

                    ARGBPixel pixel = pixels[line][i];

                    double red = 0;
                    double green = 0;
                    double blue = 0;
                    double alpha = 0;

                    for (int l = -radius; l <= radius; l++) {

                        double g = kernel[l + radius];

                        int y = line;

                        int x = i + l;
                    
                        if (x < 0) {
                            x = 0;
                        } else if (x >= Width) {
                            x = Width - 1;
                        }

                        ARGBPixel pixelToAdd = pixelsCopy[y][x];

                        red += (g * pixelToAdd.Red);
                        green += (g * pixelToAdd.Green);
                        blue += (g * pixelToAdd.Blue);
                        alpha += (g * pixelToAdd.Alpha);

                    }

                    pixel.Red = (byte)red;
                    pixel.Green = (byte)green;
                    pixel.Blue = (byte)blue;
                    pixel.Alpha = (byte)alpha;

                }

            }

        }

        /// <summary>
        /// Apply vertical motion blur.
        /// </summary>
        /// <param name="diameter">Diameter (in case kernel is null)</param>
        /// <param name="kernel">Kernel to use</param>
        /// <param name="radius">Radius of motion blur</param>
        private void ApplyMotionBlurVertically(int diameter, double[] kernel, int radius) {

            ARGBPixel[][] pixelsCopy = GetPixelsCopy();

            if (kernel == null) {

                GaussianBlurHelper gaussionBlurHelper = new GaussianBlurHelper(diameter);
                radius = gaussionBlurHelper.Radius;

                kernel = gaussionBlurHelper.OneDimensionalFilterKernel();

            }

            for (int line = 0; line < this.Height; line++) {

                for (int i = 0; i < this.Width; i++) {

                    ARGBPixel pixel = pixels[line][i];

                    double red = 0;
                    double green = 0;
                    double blue = 0;
                    double alpha = 0;

                    for (int l = -radius; l <= radius; l++) {

                        double g = kernel[l + radius];

                        int y = line + l;

                        if (y < 0) {
                            y = 0;
                        } else if (y >= Height) {
                            y = Height - 1;
                        }

                        int x = i;

                        ARGBPixel pixelToAdd = pixelsCopy[y][x];

                        red += (g * pixelToAdd.Red);
                        green += (g * pixelToAdd.Green);
                        blue += (g * pixelToAdd.Blue);
                        alpha += (g * pixelToAdd.Alpha);

                    }

                    pixel.Red = (byte)red;
                    pixel.Green = (byte)green;
                    pixel.Blue = (byte)blue;
                    pixel.Alpha = (byte)alpha;

                }

            }

        }

    }

}
