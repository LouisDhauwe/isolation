using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Functions {
    
    public class BounceFunction {

        /// <summary>
        /// Get the value 'y' for a bounce function at f(x).
        /// Where the value 'y' is the percentage of the animation to perform
        /// and x is the percentage of completion
        /// (1 = 100%, 0 = 0%)
        /// </summary>
        /// <param name="x">x value for function (between 0 and 1)</param>
        /// <returns>Value of bounce function at f(x)</returns>
        public static double Value(double x) {

            double retVal = 0;

            if (x < 0.447) {

                retVal = Math.Pow(x, 2) / 0.2;

            } else if (x < 0.7939) {

                retVal = (Math.Pow((x - 0.62), 2) / 0.1) + 0.7;

            } else if (x < 0.9995) {

                retVal = (Math.Pow((x - 0.897), 2) / 0.103) + 0.9;

            } else {

                retVal = 1;

            }

            return retVal;
        }

    }

}
