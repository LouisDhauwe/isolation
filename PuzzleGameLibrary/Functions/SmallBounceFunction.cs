using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Functions {

    public class SmallBounceFunction {

        /// <summary>
        /// Get the value 'y' for a small bounce function at f(x).
        /// Where the value 'y' is the percentage of the animation to perform
        /// and x is the percentage of completion
        /// (1 = 100%, 0 = 0%)
        /// </summary>
        /// <param name="x">x value for function (between 0 and 1)</param>
        /// <returns>Value of small bounce function at f(x)</returns>
        public static double Value(double x) {

            double retVal = 0;

            if (x < 0.7) {

                retVal = Math.Pow(x, 2) / 0.49;

            } else if (x < 1) {

                retVal = (Math.Pow((x - 0.85), 2) / 0.09) + 0.75;

            } else {

                retVal = 1;

            }

            return retVal;
        }

    }

}
