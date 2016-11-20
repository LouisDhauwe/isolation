using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Helpers {
    public class BenchmarkTimer {

        // private fields to keep track of start and end of benchmark
        private static DateTime startDate = DateTime.MinValue;
        private static DateTime endDate = DateTime.MinValue;

        /// <summary>
        /// Time interval between start and end of benchmark
        /// </summary>
        public static TimeSpan Span { 
            get { return endDate.Subtract(startDate); } 
        }

        /// <summary>
        /// Start of benchmark
        /// </summary>
        public static void Start() { 
            startDate = DateTime.Now; 
        }

        /// <summary>
        /// End of benchmark
        /// </summary>
        public static void End() { 
            endDate = DateTime.Now; 
        }

        /// <summary>
        /// Get benchmark time in seconds
        /// </summary>
        /// <returns>Benchmark time in seconds</returns>
        public static double GetSeconds() {
            if (endDate == DateTime.MinValue) {
                return 0.0;
            } else {
                return Span.TotalSeconds;
            }
        }

    }
}
