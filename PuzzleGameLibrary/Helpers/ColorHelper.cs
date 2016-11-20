﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PuzzleGameLibrary.Helpers {
    public static class ColorHelper {

        /// <summary>
        /// Convert a color in the color space AHSB to a 
        /// System.Drawing.Color object
        /// source: http://blogs.msdn.com/b/cjacks/archive/2006/04/12/575476.aspx
        /// </summary>
        /// <param name="a">Alpha channel value of color to convert</param>
        /// <param name="h">Hue channel value of color to convert</param>
        /// <param name="s">Saturation channel value of color to convert</param>
        /// <param name="b">Brightness channel value of color to convert</param>
        /// <returns></returns>
        public static Color ColorFromAhsb(int a, float h, float s, float b) {

            if (0 > a || 255 < a) {
                throw new ArgumentOutOfRangeException("InvalidAlpha");
            }
            if (0f > h || 360f < h) {
                throw new ArgumentOutOfRangeException("InvalidHue");
            }
            if (0f > s || 1f < s) {
                throw new ArgumentOutOfRangeException("InvalidSaturation");
            }
            if (0f > b || 1f < b) {
                throw new ArgumentOutOfRangeException("InvalidBrightness");
            }

            if (0 == s) {
                return Color.FromArgb(a, Convert.ToInt32(b * 255),
                  Convert.ToInt32(b * 255), Convert.ToInt32(b * 255));
            }

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < b) {
                fMax = b - (b * s) + s;
                fMin = b + (b * s) - s;
            } else {
                fMax = b + (b * s);
                fMin = b - (b * s);
            }

            iSextant = (int)Math.Floor(h / 60f);
            if (300f <= h) {
                h -= 360f;
            }
            h /= 60f;
            h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2) {
                fMid = h * (fMax - fMin) + fMin;
            } else {
                fMid = fMin - h * (fMax - fMin);
            }

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant) {
                case 1:
                    return Color.FromArgb(a, iMid, iMax, iMin);
                case 2:
                    return Color.FromArgb(a, iMin, iMax, iMid);
                case 3:
                    return Color.FromArgb(a, iMin, iMid, iMax);
                case 4:
                    return Color.FromArgb(a, iMid, iMin, iMax);
                case 5:
                    return Color.FromArgb(a, iMax, iMin, iMid);
                default:
                    return Color.FromArgb(a, iMax, iMid, iMin);
            }
        }

    }
}
