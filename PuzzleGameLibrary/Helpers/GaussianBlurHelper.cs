using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Helpers {

    public class GaussianBlurHelper {

        // Constants for easy access
        private const double PI = Math.PI;
        private const double E = Math.E;
        private readonly double SIGMA;

        // Pre-calculatable constants for later use in formulas
        private readonly double SIGMA_2;
        private readonly double FRACTION_1;

        // diameter (in pixels) of the gaussion blur
        private int diameter;

        // private fields
        private double[][] _filter2DKernel;
        private int _radius;

        /// <summary>
        /// 2D Gaussion Blur Kernel
        /// </summary>
        public double[][] Filter2DKernel {
            get { return _filter2DKernel; }
        }

        /// <summary>
        /// Radius (in pixels) of the gaussion blur
        /// </summary>
        public int Radius {
            get { return _radius; }
        }

        /// <summary>
        /// Create Gaussion Blur Helper with given diameter
        /// </summary>
        /// <param name="diameter">Diameter (in pixels) of gaussion blur</param>
        public GaussianBlurHelper(int diameter) {

            this.diameter = diameter;
            _radius = (int)Math.Floor(diameter / 2.0);

            // http://docs.opencv.org/modules/imgproc/doc/filtering.html#getgaussiankernel
            SIGMA = 0.3 * ((this.diameter - 1) * 0.5 - 1) + 0.8;

            SIGMA_2 = Math.Pow(SIGMA, 2);

            FRACTION_1 = 1.0 / (2 * PI * SIGMA_2);

            create2DKernel();

        }

        private void create2DKernel() {

            _filter2DKernel = new double[diameter][];

            double sum = 0;

            for (int k = -_radius; k <= _radius; k++) {

                for (int l = -_radius; l <= _radius; l++) {

                    double g = GaussionWeight(k, l);
                    sum += g;

                    if (_filter2DKernel[k + _radius] == null) {
                        _filter2DKernel[k + _radius] = new double[diameter];
                    }

                    _filter2DKernel[k + _radius][l + _radius] = g;

                }

            }

            // prevent darkening (since sum isn't equal to 1.0)
            for (int i = 0; i < diameter; i++) {

                for (int j = 0; j < diameter; j++) {

                    _filter2DKernel[i][j] /= sum;

                }

            }
        }

        private double GaussionWeight(int x, int y) {

            double fraction2 = - ((Math.Pow(x, 2) + Math.Pow(y, 2)) / (2 * SIGMA_2));

            double g = FRACTION_1 * Math.Pow(E, fraction2);

            return g;

        }

        /// <summary>
        /// Calculates one dimensional kernel (from 2D)
        /// </summary>
        /// <returns>1D kernel (can be used for motion blur)</returns>
        public double[] OneDimensionalFilterKernel() {

            double[] oneDfilterKernel = new double[diameter];

            for (int i = 0; i < diameter; i++) {

                for (int j = 0; j < diameter; j++) {

                    double g = _filter2DKernel[i][j];
                    oneDfilterKernel[i] += g;

                }

            }

            return oneDfilterKernel;

        }

    }

}
