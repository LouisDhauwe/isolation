using PuzzleGameLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Model {

    /// <summary>
    /// Save game object
    /// </summary>
    public class SaveGame {

        // private fields
        private GameModesData.GameModes _gameMode;
        private int _timeOffset;
        private RectIndices _indices;

        /// <summary>
        /// Game mode
        /// </summary>
        public GameModesData.GameModes GameMode {
            get { return _gameMode; }
            set { _gameMode = value; } 
        }

        /// <summary>
        /// Time offset
        /// </summary>
        public int TimeOffset {
            get { return _timeOffset; }
            set { _timeOffset = value; }
        }

        /// <summary>
        /// Indices
        /// </summary>
        public RectIndices Indices {
            get { return _indices; }
            set { _indices = value; }
        }

        /// <summary>
        /// Initialize SaveGame object
        /// </summary>
        /// <param name="gameMode">Game mode</param>
        /// <param name="timeOffset">Time offset</param>
        /// <param name="indices">Indices</param>
        public SaveGame(GameModesData.GameModes gameMode, int timeOffset, RectIndices indices) {
            _gameMode = gameMode;
            _timeOffset = timeOffset;
            _indices = indices;
        }

        /// <summary>
        /// Initialize SaveGame object from saved string
        /// </summary>
        /// <param name="savedStr"></param>
        public SaveGame(string savedStr) {

            string[] parts = savedStr.Split(new char[] { '{', '}' });
            string[] part1 = parts[1].Split(',');
            _gameMode = (GameModesData.GameModes)Int32.Parse(parts[1]);
            _timeOffset = Int32.Parse(parts[5]);

            _indices = new RectIndices(parts[3], _gameMode);

        }

        /// <summary>
        /// Get string representation of object.
        /// Can be used as save value and can be restored
        /// with designated constructor.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString() {
            string retVal = "";
            retVal += "{" + (int)_gameMode + "},";
            retVal += _indices.ToString() + ",";
            retVal += "{" + _timeOffset + "}";
            return retVal;
        }

    }
}
