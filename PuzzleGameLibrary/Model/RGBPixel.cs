using PuzzleGameLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Model {

    public class RGBPixel : Pixel {

        // private channel fields
        private byte _red, _green, _blue;

        /// <summary>
        /// Red channel value (in range 0-255)
        /// </summary>
        public byte Red {
            get { return _red; }
            set { _red = value; }
        }

        /// <summary>
        /// Green channel value (in range 0-255)
        /// </summary>
        public byte Green {
            get { return _green; }
            set { _green = value; }
        }

        /// <summary>
        /// Blue channel value (in range 0-255)
        /// </summary>
        public byte Blue {
            get { return _blue; }
            set { _blue = value; }
        }

        /// <summary>
        /// Initialize a pixel
        /// </summary>
        /// <param name="red">Red channel value</param>
        /// <param name="green">Green channel value</param>
        /// <param name="blue">Blue channel value</param>
        public RGBPixel(byte red, byte green, byte blue) : base() {
            _red = red;
            _green = green;
            _blue = blue;
        }

        /// <summary>
        /// Invert the color of the pixel
        /// </summary>
        public void InvertPixel() {
            _red = (byte) Math.Abs(_red - 255);
            _green = (byte) Math.Abs(_green - 255);
            _blue = (byte) Math.Abs(_blue - 255);
        }

        /// <summary>
        /// Make this pixel greyscale.
        /// Source: http://docs.gimp.org/2.6/en/gimp-tool-desaturate.html
        /// </summary>
        public void MakeGreyscale() {
            // Luminosity greyscale
            byte g = (byte)((0.21 * _red + 0.72 * _green + 0.07 * _blue));
            _red = g;
            _green = g;
            _blue = g;
        }

        /// <summary>
        /// Blend color with this pixel.
        /// Source: http://gimp-savvy.com/BOOK/index.html?node55.html
        /// </summary>
        /// <param name="color">Color for blending</param>
        public void BlendColor(Color color) {

            Color current = System.Drawing.Color.FromArgb(_red, _green, _blue);

            Color newColor = ColorHelper.ColorFromAhsb(255, color.GetHue(), color.GetSaturation(), current.GetBrightness());

            _red = newColor.R;
            _green = newColor.G;
            _blue = newColor.B;

        }

        /// <summary>
        /// Make this pixel sepia
        /// </summary>
        public void MakeSepia() {
            BlendColor(Color.FromArgb(172, 122, 51));
        }

    }
}
