using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Model {
    
    public class ARGBPixel : RGBPixel {

        private byte _alpha;

        /// <summary>
        /// Alpha channel value (in range 0-255)
        /// </summary>
        public byte Alpha {
            get { return _alpha; }
            set { _alpha = value; }
        }

        /// <summary>
        /// Initialize a pixel
        /// </summary>
        /// <param name="alpha">Alpha channel value</param>
        /// <param name="red">Red channel value</param>
        /// <param name="green">Green channel value</param>
        /// <param name="blue">Blue channel value</param>
        public ARGBPixel(byte alpha, byte red, byte green, byte blue)
        : base(red, green, blue) {
            _alpha = alpha;
        }

        /// <summary>
        /// Default constructor (white)
        /// </summary>
        public ARGBPixel()
        : base(255, 255, 255) {
            _alpha = 255;
        }

        /// <summary>
        /// Get transparent pixel
        /// </summary>
        /// <returns>Transparent instance</returns>
        public static ARGBPixel TransparentPixel() {
            return new ARGBPixel(0, 0, 0, 0);
        }

    }

}
