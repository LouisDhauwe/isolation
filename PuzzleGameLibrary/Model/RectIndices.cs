using PuzzleGameLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PuzzleGameLibrary.Model {

    /// <summary>
    /// Indices to represent a 2D matric of integers
    /// </summary>
    public class RectIndices {

        private int[][] indices;
        private GameModesData.GameModes gameMode;

        /// <summary>
        /// Initialize RectIndices object
        /// </summary>
        /// <param name="collumns">Collumns</param>
        /// <param name="rows">Rows</param>
        public RectIndices(int collumns, int rows) {
            FillIndices(collumns, rows);
        }

        /// <summary>
        /// Initialize RectIndices object with 2d int array
        /// </summary>
        /// <param name="indices">Indices as 2d int array</param>
        public RectIndices(int[][] indices) {
            this.indices = indices;
        }

        /// <summary>
        /// Initialize RectIndices object with indices as string and game mode
        /// </summary>
        /// <param name="savedValue">Indices as string</param>
        /// <param name="gameMode">Game mode</param>
        public RectIndices(string savedValue, GameModesData.GameModes gameMode) {
            this.gameMode = gameMode;

            int collumns = GameModesData.GetCollumns(gameMode);
            int rows = GameModesData.GetRows(gameMode);

            string[] savedIndices = savedValue.Split(',');

            indices = new int[rows][];

            int k = 0;
            for (int i = 0; i < rows; i++) {

                if (indices[i] == null) {
                    indices[i] = new int[collumns];
                }

                for (int j = 0; j < collumns; j++) {

                    SetIndex(i, j, int.Parse(savedIndices[k]));

                    k++;
                }

            }

        }

        private void FillIndices(int collumns, int rows) {

            indices = new int[rows][];

            int k = 0;
            for (int i = 0; i < rows; i++) {

                if (indices[i] == null) {
                    indices[i] = new int[collumns];
                }

                for (int j = 0; j < collumns; j++) {

                    SetIndex(i, j, k);

                    k++;
                }

            }

        }

        /// <summary>
        /// Get index
        /// </summary>
        /// <param name="collumn">Collumn</param>
        /// <param name="row">Row</param>
        /// <returns></returns>
        public int GetIndex(int collumn, int row) {
            return indices[collumn][row];
        }

        /// <summary>
        /// Set index at given collumn, row
        /// </summary>
        /// <param name="collumn">Collumn</param>
        /// <param name="row">Row</param>
        /// <param name="value">Index to set at collumn, row</param>
        public void SetIndex(int collumn, int row, int value) {
            indices[collumn][row] = value;
        }

        /// <summary>
        /// Swap 2 indices
        /// </summary>
        /// <param name="collumn1">Collumn of first index</param>
        /// <param name="row1">Row of first index</param>
        /// <param name="collumn2">Collumn of second index</param>
        /// <param name="row2">Row of second index</param>
        public void SwapIndices(int collumn1, int row1, int collumn2, int row2) {
            int temp = indices[collumn1][row1];
            indices[collumn1][row1] = indices[collumn2][row2];
            indices[collumn2][row2] = temp;
        }

        /// <summary>
        /// Get number of rows
        /// </summary>
        /// <returns>Rows</returns>
        public int Rows() {
            return indices.Length;
        }

        /// <summary>
        /// Get number of collumns
        /// </summary>
        /// <returns>Collumns</returns>
        public int Collumns() {
            return indices[0].Length;
        }

        /// <summary>
        /// Get string representation of object.
        /// Can be used as save value and can be restored
        /// with designated constructor.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString() {
            string retVal = "{";

            bool firstIndex = true;
            for (int i = 0; i < Rows(); i++) {

                for (int j = 0; j < Collumns(); j++) {

                    int index = GetIndex(i, j);

                    if (!firstIndex) {
                        retVal += ",";
                    } else {
                        firstIndex = false;
                    }

                    retVal += index;
                }

            }

            retVal += "}";
            return retVal;
        }

        /// <summary>
        /// Get Point (row, collumn) of index.
        /// Will return first found if multiple with same index
        /// </summary>
        /// <param name="index">Index to search</param>
        /// <returns>Point with row and collumn values</returns>
        public Point PointOfIndex(int index) {

            for (int i = 0; i < Rows(); i++) {

                for (int j = 0; j < Collumns(); j++) {

                    if (indices[i][j] == index) {

                        return new Point(i, j);

                    }

                }
            }

            return new Point(0, 0);
        }

        /// <summary>
        /// Copy indices object (deep copy)
        /// </summary>
        /// <returns>Deep copy of object</returns>
        public RectIndices Copy() {
            int[][] indicesCopy = indices.Select(a => a.ToArray()).ToArray();
            return new RectIndices(indicesCopy);
        }

        /// <summary>
        /// Checks if instance equals other instance
        /// </summary>
        /// <param name="obj">Instance to compare to</param>
        /// <returns>True if equal</returns>
        public override bool Equals(Object obj) {
            RectIndices rectIndicesObj = obj as RectIndices;

            if (rectIndicesObj != null) {

                if (Rows() == rectIndicesObj.Rows() && 
                    Collumns() == rectIndicesObj.Collumns()) {

                    bool equal = true;

                    for (int i = 0; i < Rows(); i++) {

                        for (int j = 0; j < Collumns(); j++) {

                            if (GetIndex(i, j) != rectIndicesObj.GetIndex(i, j)) {
                                equal = false;
                            }

                        }

                    }

                    return equal;

                }

            }

            return false;

        }

        /// <summary>
        /// Get hash code (for hash tables, etc.)
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode() {
            // Recommended to use some prime numbers
            int hash = 17;
            hash += 23 * indices.GetHashCode();
            return hash;
        }

    }
}
