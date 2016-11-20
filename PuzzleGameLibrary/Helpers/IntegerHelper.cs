using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Helpers {

    public class IntegerHelper {

        /// <summary>
        /// Rounds given double upwards (to integer)
        /// </summary>
        /// <param name="i">Double to round upwards</param>
        /// <returns>Upwards rounded double (as an int)</returns>
        public static int Ceil(double i) {
            return (int)(Math.Ceiling(i) + 0.5);
        }

        /// <summary>
        /// Calculates sign of given integer.
        /// -1 = negative,
        ///  0 = no sign,
        ///  1 = positive
        /// </summary>
        /// <param name="x">Integer to calculate sign of</param>
        /// <returns>Sign as described above</returns>
        public static int SignOf(int x) {
            if (x > 0) return 1;
            if (x < 0) return -1;
            return 0;
        }

    }

}
