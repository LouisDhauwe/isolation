using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Functions {

    public class EaseInOutFunction {

        /// <summary>
        /// Get the value 'y' for a ease in out function at f(x).
        /// Where the value 'y' is the percentage of the animation to perform
        /// and x is the percentage of completion
        /// (1 = 100%, 0 = 0%)
        /// </summary>
        /// <param name="x">x value for function (between 0 and 1)</param>
        /// <returns>Value of ease in out function at f(x)</returns>
        public static double Value(double x) {
            return Math.Cos((x + 1) * Math.PI) / 2.0 + 0.5;
        }

    }

}
