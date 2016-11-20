using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleGameLibrary.Model;
using PuzzleGameLibrary.Helpers;
using PuzzleGameLibrary.Functions;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;
using PuzzleGameLibrary.Data;

namespace PuzzleGameLibrary {

    /// <summary>
    /// Game board that handles slide puzzle logic and tile animations
    /// </summary>
    public class GameBoard : IDisposable {

        // constants
        private readonly Bitmap ORIGINAL_BITMAP;

        private readonly int TILES_HORIZONTALLY;
        private readonly int TILES_VERTICALLY;

        private readonly int TILE_WIDTH;
        private readonly int TILE_HEIGHT;

        private readonly int BORDER_WIDTH;

        private readonly GameModesData.GameModes GAME_MODE;

        private const int EMPTY_TILE = -1;

        // Directions
        private const int UP = 0;
        private const int DOWN = 1;
        private const int LEFT = 2;
        private const int RIGHT = 3;

        private BackgroundWorker blurRenderWorker;

        private ARGBBitmapArray bitmapArray;
        private ARGBBitmapArray shadowBitmapArray;
        private ARGBBitmapArray shadowWithoutBordersBitmapArray;

        private Dictionary<int, ARGBBitmapArray> blurBitmapArrays;

        private RectIndices solutionTiles;

        private Queue<int[][]> FrameAnimations;

        private Model.Functions _animationFunction;

        private RectIndices _tiles;

        private int _frameRate;

        /// <summary>
        /// Tile indices of game
        /// </summary>
        public RectIndices Tiles {
            get { return _tiles; }
        }

        /// <summary>
        /// Game image width
        /// </summary>
        public int Width {
            get { return bitmapArray.Width; }
        }

        /// <summary>
        /// Game image Height
        /// </summary>
        public int Height {
            get { return bitmapArray.Height; }
        }

        /// <summary>
        /// Border width (used for shadows)
        /// </summary>
        public int BorderWidth {
            get { return BORDER_WIDTH; }
        }

        /// <summary>
        /// Animation function for frame calculations
        /// </summary>
        public Model.Functions AnimationFunction {
            get { return _animationFunction; }
            set { _animationFunction = value; }
        }

        /// <summary>
        /// Frame rate used for frame precision calculations
        /// </summary>
        public int FrameRate {
            get { return _frameRate; }
            set { _frameRate = value; } 
        }

        /// <summary>
        /// Initialize GameBoard with SaveGame object.
        /// Will throw exception if SaveGame is not valid
        /// </summary>
        /// <param name="bitmap">Bitmap to use as game image</param>
        /// <param name="saveGame">SaveGame object</param>
        /// <param name="borderWidth">Border width (used for shadows)</param>
        public GameBoard(Bitmap bitmap, SaveGame saveGame, int borderWidth) {
            
            try {

                bitmapArray = new ARGBBitmapArray(bitmap);

                GAME_MODE = saveGame.GameMode;

                ORIGINAL_BITMAP = (Bitmap)bitmap.Clone();

                BORDER_WIDTH = borderWidth;

                TILES_VERTICALLY = GameModesData.GetRows(GAME_MODE);
                TILES_HORIZONTALLY = GameModesData.GetCollumns(GAME_MODE);

                TILE_WIDTH = (int)(bitmapArray.Width / TILES_HORIZONTALLY);
                TILE_HEIGHT = (int)(bitmapArray.Height / TILES_VERTICALLY);

                Setup(bitmap, false);

                SwapAllTiles(this._tiles, saveGame.Indices);
                this._tiles = saveGame.Indices;

            } catch {
                CleanUp();
                throw new FormatException();
            }
     
        }

        /// <summary>
        /// Initialize GameBoard
        /// </summary>
        /// <param name="bitmap">Bitmap to use as game image</param>
        /// <param name="gameMode">Game mode</param>
        /// <param name="borderWidth">Border width (used for shadows)</param>
        public GameBoard(Bitmap bitmap, GameModesData.GameModes gameMode, int borderWidth) {

            GAME_MODE = gameMode;

            bitmapArray = new ARGBBitmapArray(bitmap);

            ORIGINAL_BITMAP = (Bitmap)bitmap.Clone();

            BORDER_WIDTH = borderWidth;

            TILES_VERTICALLY = GameModesData.GetRows(GAME_MODE);
            TILES_HORIZONTALLY = GameModesData.GetCollumns(GAME_MODE);

            TILE_WIDTH = (int)(bitmapArray.Width / TILES_HORIZONTALLY);
            TILE_HEIGHT = (int)(bitmapArray.Height / TILES_VERTICALLY);

            Setup(bitmap, true);

        }

        private void Setup(Bitmap bitmap, bool randomize) {

            bitmapArray.Crop((TILES_VERTICALLY * TILE_HEIGHT), (TILES_HORIZONTALLY * TILE_WIDTH));

            FrameAnimations = new Queue<int[][]>();

            FrameRate = 60;
            makeTileIndices();

            MakeEmptyTile(bitmapArray);

            if (randomize) {
                Randomize();
            }

            CreateShadowLayers();

            blurBitmapArrays = new Dictionary<int, ARGBBitmapArray>();
            PreRenderGaussionBlurs();

        }

        /// <summary>
        /// Cleanup use of bitmap
        /// </summary>
        public void CleanUp() {
            bitmapArray.CleanUp();
        }

        private void PreRenderGaussionBlurs() {

            blurRenderWorker = new BackgroundWorker();

            blurRenderWorker.DoWork += new DoWorkEventHandler(BlurRenderDoWork);
            blurRenderWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BlurRenderRunWorkerCompleted);
            blurRenderWorker.WorkerSupportsCancellation = true;

            blurRenderWorker.RunWorkerAsync();

        }

        private void BlurRenderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            Console.WriteLine("Blur Renders Completed");
        }

        private void BlurRenderDoWork(object sender, DoWorkEventArgs e) {

            for (int i = 3; i < 51; i += 2) {

                Bitmap newBmp = (Bitmap)ORIGINAL_BITMAP.Clone();

                ARGBBitmapArray bmpArray = new ARGBBitmapArray(newBmp);
                bmpArray.Crop((TILES_VERTICALLY * TILE_HEIGHT), (TILES_HORIZONTALLY * TILE_WIDTH));

                bmpArray.ApplyFastGaussianBlur(i);
                MakeEmptyTile(bmpArray);

                blurBitmapArrays.Add(i, bmpArray);

            }
            
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        private void CreateShadowLayers() {

            System.Drawing.Bitmap shadowBmp = new System.Drawing.Bitmap((int)bitmapArray.Width, (int)bitmapArray.Height);
            System.Drawing.Bitmap shadowWithoutBordersBmp = new System.Drawing.Bitmap((int)bitmapArray.Width, (int)bitmapArray.Height);

            shadowBitmapArray = new ARGBBitmapArray(shadowBmp);
            shadowWithoutBordersBitmapArray = new ARGBBitmapArray(shadowWithoutBordersBmp);


            double fractionW = (int)bitmapArray.Width / (double)TILES_HORIZONTALLY;
            double fractionH = (int)bitmapArray.Height / (double)TILES_VERTICALLY;

            ARGBPixel shadowColor = new ARGBPixel(100, 0, 0, 0);

            for (int i = 0; i <= TILES_HORIZONTALLY; i++) {

                Rect r = new Rect(fractionW * i - BORDER_WIDTH/2,
                                0,
                                BORDER_WIDTH,
                                (int)bitmapArray.Height-1);


                shadowBitmapArray.FillRect(r, shadowColor);

                if (i == 0 || i == TILES_HORIZONTALLY) {
                    shadowWithoutBordersBitmapArray.FillRect(r, shadowColor);
                }

            }

            for (int i = 0; i <= TILES_VERTICALLY; i++) {

                Rect r = new Rect(0,
                                fractionH * i - BORDER_WIDTH / 2,
                                (int)bitmapArray.Width - 1,
                                BORDER_WIDTH);


                shadowBitmapArray.FillRect(r, shadowColor);

                if (i == 0 || i == TILES_VERTICALLY) {
                    shadowWithoutBordersBitmapArray.FillRect(r, shadowColor);
                }
            }

            BenchmarkTimer.Start();

            shadowBitmapArray.ApplyFastGaussianBlur(15);
            shadowWithoutBordersBitmapArray.ApplyFastGaussianBlur(15);

            BenchmarkTimer.End();

            Console.WriteLine(BenchmarkTimer.GetSeconds());

        }

        /// <summary>
        /// Get shadow layer with the borders
        /// </summary>
        /// <returns>Shadow as BitmapImage</returns>
        public BitmapImage GetShadowLayer() {
            return shadowBitmapArray.GetBitmapImage();
        }

        /// <summary>
        /// Get the shadow layer without the borders (only edges)
        /// </summary>
        /// <returns>Shadow as BitmapImage</returns>
        public BitmapImage GetShadowWithoutBordersLayer() {
            return shadowWithoutBordersBitmapArray.GetBitmapImage();
        }

        /// <summary>
        /// Checks if puzzle is solved
        /// </summary>
        /// <returns>True if solved</returns>
        public bool IsPuzzleSolved() {
            return _tiles.Equals(solutionTiles);
        }

        /// <summary>
        /// Make a move by sliding down
        /// </summary>
        public void MoveDown() {

            System.Windows.Point emptyTile = _tiles.PointOfIndex(EMPTY_TILE);

            int line = (int)emptyTile.X - 1;
            int row = (int)emptyTile.Y;

            MoveTile(line, row);

        }

        /// <summary>
        /// Make a move by sliding up
        /// </summary>
        public void MoveUp() {

            System.Windows.Point emptyTile = _tiles.PointOfIndex(EMPTY_TILE);

            int line = (int)emptyTile.X + 1;
            int row = (int)emptyTile.Y;

            MoveTile(line, row);

        }

        /// <summary>
        /// Make a move by sliding left
        /// </summary>
        public void MoveLeft() {

            System.Windows.Point emptyTile = _tiles.PointOfIndex(EMPTY_TILE);

            int line = (int)emptyTile.X;
            int row = (int)emptyTile.Y + 1;

            MoveTile(line, row);

        }

        /// <summary>
        /// Make a move by sliding right
        /// </summary>
        public void MoveRight() {

            System.Windows.Point emptyTile = _tiles.PointOfIndex(EMPTY_TILE);

            int line = (int)emptyTile.X;
            int row = (int)emptyTile.Y - 1;

            MoveTile(line, row);

        }
        
        // First parameter of tuple = true if empty space is found,
        // false if not found
        private Tuple<bool, int, int> GetEmptySpaceNextToTile(int line, int i) {

            bool emptySpaceFound = false;
            int lineE = 0;
            int iE = 0;

            if (TileIsEmpty(line - 1, i, ref lineE, ref iE) ||
                TileIsEmpty(line + 1, i, ref lineE, ref iE) ||
                TileIsEmpty(line, i + 1, ref lineE, ref iE) ||
                TileIsEmpty(line, i - 1, ref lineE, ref iE)) {

                emptySpaceFound = true;

            }

            return Tuple.Create(emptySpaceFound, lineE, iE);

        }

        private bool TileIsEmpty(int line, int i, ref int lineE, ref int iE) {

            if (TileIndexInBounds(line, i)) {

                if (_tiles.GetIndex(line, i) == EMPTY_TILE) {
                    lineE = line;
                    iE = i;
                    return true;
                }

            }

            return false;

        }

        private bool TileIndexInBounds(int line, int i) {

            if (line < 0 ||
                i < 0 ||
                line >= TILES_VERTICALLY ||
                i >= TILES_HORIZONTALLY) {

                return false;

            }

            return true;

        }

        private void makeTileIndices() {

            _tiles = new RectIndices(TILES_HORIZONTALLY, TILES_VERTICALLY);
            _tiles.SetIndex(0, TILES_HORIZONTALLY - 1, EMPTY_TILE);

            solutionTiles = _tiles.Copy();

        }

        private int LineClicked(int y) {
            return (int)Math.Floor(y / (double)TILE_HEIGHT);
        }

        private int RowClicked(int x) {
            return (int)Math.Floor(x / (double)TILE_WIDTH);
        }

        /// <summary>
        /// Check if click at x, y is a valid move
        /// </summary>
        /// <param name="x">X coordinate in game image</param>
        /// <param name="y">Y coordinate in game image</param>
        /// <returns>True if valid</returns>
        public bool IsValidClick(int x, int y) {

            int lineClicked = LineClicked(y);
            int rowClicked = RowClicked(x);
            Tuple<bool, int, int> t = GetEmptySpaceNextToTile(lineClicked, rowClicked);

            return t.Item1;
        }

        /// <summary>
        /// Checks if click is in bounds of game image
        /// </summary>
        /// <param name="x">X coordinate in game image</param>
        /// <param name="y">Y coordinate in game image</param>
        /// <returns>True if in bounds</returns>
        public bool IsClickInBounds(int x, int y) {

            if (x < 0 ||
                y < 0 ||
                x >= Width ||
                y >= Height) {
                
                return false;
            }

            return true;

        }

        /// <summary>
        /// Process click on board
        /// </summary>
        /// <param name="x">X coordinate in game image</param>
        /// <param name="y">Y coordinate in game image</param>
        public void BoardClicked(int x, int y) {

            // check if invalid move
            if (!IsClickInBounds(x, y)) {
                return;
            }

            int lineClicked = LineClicked(y);
            int rowClicked = RowClicked(x);

            MoveTile(lineClicked, rowClicked);

        }

        private void MoveTile(int line, int row) {

            if (TileIndexInBounds(line, row)) {

                Tuple<bool, int, int> t = GetEmptySpaceNextToTile(line, row);
                if (t.Item1) {
                    SwapTiles(line, row, t.Item2, t.Item3, true);
                }

            }

        }

        /// <summary>
        /// Checks if game board has frame update
        /// </summary>
        /// <returns>True if frame update available</returns>
        public bool HasFrameUpdate() {
            return FrameAnimations.Count > 0;
        }

        /// <summary>
        /// Update frame
        /// </summary>
        public void UpdateFrame() {

            if (HasFrameUpdate()) {

                int[][] currentFrame = FrameAnimations.Dequeue();

                foreach (int[] translation in currentFrame) {

                    bitmapArray.SwapPixels(translation[0],
                                            translation[1],
                                            translation[2],
                                            translation[3]);

                }

            }

        }

        private void MakeEmptyTile(ARGBBitmapArray bmpArray) {

            ARGBPixel bgColor = new ARGBPixel(128, 224, 181, 130);

            // erase top right tile (so you can slide tiles, duh)
            for (int line = 0; line < TILE_HEIGHT; line++) {

                for (int i = (int)(TILE_WIDTH) * (TILES_HORIZONTALLY - 1); i < bmpArray.Width; i++) {

                    bmpArray.SetPixel(line, i, bgColor);

                }

            }
        }

        private void Randomize() {

            Random rnd = new Random();

            int prevLine = 0;
            int prevRow = 0;

            int i = 0;
            while (i < Math.Pow((TILES_VERTICALLY * TILES_HORIZONTALLY), 2)) {

                int line = rnd.Next(0, TILES_VERTICALLY);
                int row = rnd.Next(0, TILES_HORIZONTALLY);

                // prevent to undo prev move
                if (line == prevLine &&
                    row == prevRow) {
                    continue;
                }

                Tuple<bool, int, int> t = GetEmptySpaceNextToTile(line, row);
                if (t.Item1) {

                    prevLine = t.Item2;
                    prevRow = t.Item3;

                    _tiles.SwapIndices(line, row, t.Item2, t.Item3);

                    if (!IsPuzzleSolved()) {
                        i++;
                    }
                }

            }

            SwapAllTiles(solutionTiles.Copy(), _tiles);

        }

        private void SwapTiles(int line1, int i1, int line2, int i2, bool animated) {

            _tiles.SwapIndices(line1, i1, line2, i2);

            if (animated) {

                AddFramesSwapTilesAnimation(line1, i1, line2, i2);

            } else {

                for (int line = TILE_HEIGHT * line1; line < TILE_HEIGHT * (line1 + 1); line++) {

                    for (int i = TILE_WIDTH * i1; i < TILE_WIDTH * (i1 + 1); i++) {

                        int newLine = line + TILE_HEIGHT * (line2 - line1);
                        int newI = i + TILE_WIDTH * (i2 - i1);

                        bitmapArray.SwapPixels(line, i, newLine, newI);

                    }

                }

            }

        }

        private double Value(double x) {

            if (AnimationFunction == Model.Functions.Bounce) {
                return BounceFunction.Value(x);

            } else if (AnimationFunction == Model.Functions.EaseIn) {
                return EaseInFunction.Value(x);

            } else if (AnimationFunction == Model.Functions.EaseInOut) {
                return EaseInOutFunction.Value(x);

            } else if (AnimationFunction == Model.Functions.EaseOut) {
                return EaseOutFunction.Value(x);

            } else if (AnimationFunction == Model.Functions.SmallBounce) {
                return SmallBounceFunction.Value(x);
            
            }

            return LinearFunction.Value(x);
        }

        private int prevAnimIndex;
        private int stepSize;
        private int currentDirection = -1;

        private void AddFramesSwapTilesAnimation(int line1, int i1, int line2, int i2) {

            prevAnimIndex = 0;
            stepSize = (int) (TILE_WIDTH / (double) 5);

            double frameFactor = - 0.2 * FrameRate + 72;
            double step = 1.0 / ((frameFactor) * 0.12);

            if (line2 - line1 < 0) {

                currentDirection = UP;

            } else if (line2 - line1 > 0) {

                currentDirection = DOWN;

            } else if (i2 - i1 < 0) {

                currentDirection = LEFT;

            } else if (i2 - i1 > 0) {

                currentDirection = RIGHT;

            }

            int target;

            if (currentDirection == LEFT || currentDirection == RIGHT) {
                target = TILE_WIDTH;
            } else {
                target = TILE_HEIGHT;
            }

            for (double x = 0; x <= 1.0 + step; x += step) {

                double y = Value(x);
                int k = IntegerHelper.Ceil(target * y);

                if (x > 1 ||
                    y >= 1.0 - 2 * step ||
                    k > target) {

                    k = target;

                }

                if (currentDirection == UP || currentDirection == DOWN) {

                    AddFrameToAnimation(line1, i1, line2, i2, k, 0);

                } else if (currentDirection == LEFT || currentDirection == RIGHT) {

                    AddFrameToAnimation(line1, i1, line2, i2, 0, k);

                }

                prevAnimIndex = k;

            }

            currentDirection = -1;

        }

        private void AddFrameToAnimation(int line1, int i1, int line2, int i2, int deltaLine, int deltaI) {

            int[][] newFrame = new int[TILE_HEIGHT * TILE_WIDTH][];
            int frameIndex = 0;

            if (currentDirection == UP) {

                if (prevAnimIndex - deltaLine > 0) {

                    for (int line = TILE_HEIGHT * (line1 + 1) - 1; line >= (TILE_HEIGHT * line1); line--) {

                        for (int i = TILE_WIDTH * i1; i < TILE_WIDTH * (i1 + 1); i++) {

                            int newLine = line + TILE_HEIGHT * (line2 - line1);
                            int newI = i + TILE_WIDTH * (i2 - i1);

                            newFrame[frameIndex] = new int[] { line - prevAnimIndex, i, newLine + (TILE_HEIGHT - deltaLine), newI };
                            frameIndex++;

                        }

                    }

                } else {

                    for (int line = (TILE_HEIGHT * line1); line < TILE_HEIGHT * (line1 + 1); line++) {

                        for (int i = TILE_WIDTH * i1; i < TILE_WIDTH * (i1 + 1); i++) {

                            int newLine = line + TILE_HEIGHT * (line2 - line1);
                            int newI = i + TILE_WIDTH * (i2 - i1);

                            newFrame[frameIndex] = new int[] { line - prevAnimIndex, i, newLine + (TILE_HEIGHT - deltaLine), newI };

                            frameIndex++;

                        }

                    }

                }

            } else if (currentDirection == DOWN) {

                if (prevAnimIndex - deltaLine > 0) {

                    for (int line = (TILE_HEIGHT * line1); line < TILE_HEIGHT * (line1 + 1); line++) {

                        for (int i = TILE_WIDTH * i1; i < TILE_WIDTH * (i1 + 1); i++) {

                            int newLine = line + TILE_HEIGHT * (line2 - line1);
                            int newI = i + TILE_WIDTH * (i2 - i1);

                            newFrame[frameIndex] = new int[] { line + prevAnimIndex, i, newLine - (TILE_HEIGHT - deltaLine), newI };

                            frameIndex++;

                        }

                    }

                } else {

                    for (int line = TILE_HEIGHT * (line1 + 1) - 1; line >= (TILE_HEIGHT * line1); line--) {

                        for (int i = TILE_WIDTH * i1; i < TILE_WIDTH * (i1 + 1); i++) {

                            int newLine = line + TILE_HEIGHT * (line2 - line1);
                            int newI = i + TILE_WIDTH * (i2 - i1);

                            newFrame[frameIndex] = new int[] { line + prevAnimIndex, i, newLine - (TILE_HEIGHT - deltaLine), newI };
                            frameIndex++;

                        }

                    }

                }

            } else if (currentDirection == LEFT) {

                if (prevAnimIndex - deltaI > 0) {

                    for (int line = (TILE_HEIGHT * line1); line < TILE_HEIGHT * (line1 + 1); line++) {

                        for (int i = TILE_WIDTH * (i1 + 1) - 1; i >= TILE_WIDTH * i1; i--) {

                            int newLine = line + TILE_HEIGHT * (line2 - line1);
                            int newI = i + TILE_WIDTH * (i2 - i1);

                            newFrame[frameIndex] = new int[] { line, i - prevAnimIndex, newLine, newI + (TILE_WIDTH - deltaI) };
                            frameIndex++;

                        }

                    }

                } else {

                    for (int line = (TILE_HEIGHT * line1); line < TILE_HEIGHT * (line1 + 1); line++) {

                        for (int i = TILE_WIDTH * i1; i < TILE_WIDTH * (i1 + 1); i++) {

                            int newLine = line + TILE_HEIGHT * (line2 - line1);
                            int newI = i + TILE_WIDTH * (i2 - i1);

                            newFrame[frameIndex] = new int[] { line, i - prevAnimIndex, newLine, newI + (TILE_WIDTH - deltaI) };
                            frameIndex++;

                        }

                    }

                }

            } else if (currentDirection == RIGHT) {

                if (prevAnimIndex - deltaI > 0) {

                    for (int line = (TILE_HEIGHT * line1); line < TILE_HEIGHT * (line1 + 1); line++) {

                        for (int i = TILE_WIDTH * i1; i < TILE_WIDTH * (i1 + 1); i++) {

                            int newLine = line + TILE_HEIGHT * (line2 - line1);
                            int newI = i + TILE_WIDTH * (i2 - i1);

                            newFrame[frameIndex] = new int[] { line, i + prevAnimIndex, newLine, newI - (TILE_WIDTH - deltaI) };
                            frameIndex++;

                        }

                    }

                } else {

                    for (int line = (TILE_HEIGHT * line1); line < TILE_HEIGHT * (line1 + 1); line++) {

                        for (int i = TILE_WIDTH * (i1 + 1) - 1; i >= TILE_WIDTH * i1; i--) {

                            int newLine = line + TILE_HEIGHT * (line2 - line1);
                            int newI = i + TILE_WIDTH * (i2 - i1);

                            newFrame[frameIndex] = new int[] { line, i + prevAnimIndex, newLine, newI - (TILE_WIDTH - deltaI) };
                            frameIndex++;

                        }

                    }

                }

            }

            FrameAnimations.Enqueue(newFrame);

        }

        private void SwapAllTiles(RectIndices indicesFrom, RectIndices indicesTo) {

            for (int i = 0; i < TILES_VERTICALLY; i++) {

                for (int j = 0; j < TILES_HORIZONTALLY; j++) {

                    if (indicesFrom.GetIndex(i, j) != indicesTo.GetIndex(i, j)) {

                        System.Windows.Point destPoint = indicesFrom.PointOfIndex(indicesTo.GetIndex(i, j));

                        SwapTiles(i, j, (int)destPoint.X, (int)destPoint.Y, false);

                        _tiles.SwapIndices(i, j, (int)destPoint.X, (int)destPoint.Y);

                        indicesFrom.SwapIndices(i, j, (int)destPoint.X, (int)destPoint.Y);

                    }

                }

            }

        }

        private int currentBlurLoaded = -1;
        private bool _updateForBlur = false;

        /// <summary>
        /// Returns true if frame should force update for blur
        /// </summary>
        public bool UpdateForBlur {
            get { return _updateForBlur; }
            set { _updateForBlur = value; }
        }

        /// <summary>
        /// Load a gaussion blur as board image
        /// </summary>
        /// <param name="radius"></param>
        public void LoadGausianBlur(int radius) {

            if (HasFrameUpdate() || blurBitmapArrays == null) return;

            if (!blurBitmapArrays.ContainsKey(radius)) {

                int nearestRadius = blurBitmapArrays.Keys.OrderBy(x => Math.Abs((long)x - radius)).First();
                radius = nearestRadius;
            }

            if (radius == currentBlurLoaded) {
                return;
            }

            currentBlurLoaded = radius;
            UpdateForBlur = true;

            if (blurBitmapArrays.ContainsKey(radius)) {

                bitmapArray = blurBitmapArrays[radius];

                RectIndices solutionCopy = solutionTiles.Copy();

                SwapAllTiles(solutionCopy, _tiles);

            }

        }

        /// <summary>
        /// Get game image as Bitmap (Windows Forms)
        /// </summary>
        /// <returns></returns>
        public Bitmap GetGameBoardBitmap() {
            return new Bitmap(bitmapArray.GetBitmapStream());
        }

        /// <summary>
        /// Get game image as BitmapImage (WPF)
        /// </summary>
        /// <returns></returns>
        public BitmapImage GetGameBoardBitmapImage() {
            return bitmapArray.GetBitmapImage();
        }
    
    }

}
