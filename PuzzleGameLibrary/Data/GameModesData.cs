using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleGameLibrary.Data {
    public static class GameModesData {

        // Easy mode constants
        private const int EASY_ROWS = 2;
        private const int EASY_COLLUMNS = 2;
        private const int EASY_TIME_SECONDS = 60;

        // Normal mode constants
        private const int NORMAL_ROWS = 3;
        private const int NORMAL_COLLUMNS = 3;
        private const int NORMAL_TIME_SECONDS = 60;

        // Hard mode constants
        private const int HARD_ROWS = 4;
        private const int HARD_COLLUMNS = 4;
        private const int HARD_TIME_SECONDS = 600;

        /// <summary>
        /// Game modes
        /// </summary>
        public enum GameModes {
            EASY, NORMAL, HARD
        };

        /// <summary>
        /// Get the number of rows for a gameboard 
        /// for the given game mode
        /// </summary>
        /// <param name="gameMode">Game mode</param>
        /// <returns>Number of rows</returns>
        public static int GetRows(GameModes gameMode) {
            switch (gameMode) {
                case GameModes.EASY:
                    return EASY_ROWS;
                case GameModes.NORMAL:
                    return NORMAL_ROWS;
                default:
                    return HARD_ROWS;
            }
        }

        /// <summary>
        /// Get the number of collumns for a gameboard 
        /// for the given game mode
        /// </summary>
        /// <param name="gameMode">Game mode</param>
        /// <returns>Number of collumns</returns>
        public static int GetCollumns(GameModes gameMode) {
            switch (gameMode) {
                case GameModes.EASY:
                    return EASY_COLLUMNS;
                case GameModes.NORMAL:
                    return NORMAL_COLLUMNS;
                default:
                    return HARD_COLLUMNS;
            }
        }

        /// <summary>
        /// Get the max. time (in seconds) for a game 
        /// in the given game mode
        /// </summary>
        /// <param name="gameMode">Game mode</param>
        /// <returns>Max. time for one game (in seconds)</returns>
        public static int GetTimeSeconds(GameModes gameMode) {
            switch (gameMode) {
                case GameModes.EASY:
                    return EASY_TIME_SECONDS;
                case GameModes.NORMAL:
                    return NORMAL_TIME_SECONDS;
                default:
                    return HARD_TIME_SECONDS;
            }
        }
    }
}
