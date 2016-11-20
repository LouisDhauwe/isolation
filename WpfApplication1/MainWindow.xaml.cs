using PuzzleGameLibrary;
using PuzzleGameLibrary.Helpers;
using PuzzleGameLibrary.Model;
using PuzzleGameLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Media.Animation;
using System.ComponentModel;
using PuzzleGame;

namespace WpfApplication1 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        // Constants
        private readonly int GAME_TIME_SECONDS;

        private DateTime GAME_START_DATE;

        private const bool MEASURE_BENCHMARK = false;

        private const int BORDER_WIDTH = 8;

        private const int SLIDE_IN = 0;
        private const int SLIDE_OUT = 1;
        private const int NONE = -1;

        private readonly int TILES_COLLUMNS = 3;
        private readonly int TILES_ROWS = 3;

        private readonly GameModesData.GameModes gameMode;

        // Game states
        private const int GAME_RUNNING = 0;
        private const int GAME_PAUSED = 1;
        private const int GAME_OVER = 2;
        private const int GAME_WON = 3;

        // private fields
        private int gameState = GAME_PAUSED;

        private DispatcherTimer frameTimer;
        private GameBoard gameBoard;

        private int windowSlideDirection = NONE;

        private string filePath;

        private int accelerateFactor = 1;
        private int frameRate = 60; // probably won't reach this high, still smooths things though

        private bool hasGameStarted = false;

        private double windowYPositionCenter;
        private ARGBBitmapArray gameOverSolutionBmpArray;

        private System.Drawing.Bitmap gameBmp;

        private int timeOffset = 0;

        public MainWindow(System.Drawing.Bitmap gameBmp, string filePath, GameModesData.GameModes gameMode) {
            InitializeComponent();

            this.gameBmp = gameBmp;
            this.filePath = filePath;
            this.gameMode = gameMode;

            TILES_COLLUMNS = GameModesData.GetCollumns(gameMode);
            TILES_ROWS = GameModesData.GetRows(gameMode);
            GAME_TIME_SECONDS = GameModesData.GetTimeSeconds(gameMode);

            if (FullScreen()) {
                ((App)Application.Current).MakeFullScreen(this);
            }

            UpdateFullscreenBtn();

            frameTimer = new System.Windows.Threading.DispatcherTimer();
            frameTimer.Interval = TimeSpan.FromMilliseconds(1000 / frameRate);
            frameTimer.IsEnabled = true;
            frameTimer.Tick += new EventHandler(FrameTick);

            RenderOptions.SetBitmapScalingMode(gameBoardImage, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(gameBoardSolutionImage, BitmapScalingMode.HighQuality);

            ProcessImage();

            // Initial values 

            gameMessageLbl.Content = "";

            TimeSpan t = TimeSpan.FromSeconds(GAME_TIME_SECONDS);

            string timeStr = string.Format("{0:D2}:{1:D2}",
                            t.Minutes,
                            t.Seconds);

            timerProgressLbl.Content = timeStr;

            timerProgress.Value = 100;

            retryBtn.Visibility = Visibility.Hidden;
            retryBtn.Opacity = 0;

            if (!SaveSupported()) {
                saveBtn.Visibility = Visibility.Hidden;
            }

        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {

            if (FullScreen()) {

                StartGame();

            } else {

                windowYPositionCenter = this.Top;
                this.Top = -SystemParameters.PrimaryScreenHeight / 2;

                windowSlideDirection = SLIDE_IN;

            }

            frameTimer.Start();

        }

        private bool SaveSupported() {

            if (filePath == "") {
                return false;
            }

            return ImageIOHelper.MetadataSupported(filePath);

        }

        private bool FullScreen() {
            return ((App)Application.Current).FullScreen;
        }

        private void ProcessImage() {

            System.Drawing.Bitmap bmp = gameBmp;

            gameBoardSolutionImage.Source = (ImageSource)ImageStreamHelper.ToBitmapSource(bmp);
            gameOverSolutionBmpArray = new ARGBBitmapArray((System.Drawing.Bitmap)bmp.Clone());
            gameOverSolutionBmpArray.ApplySepia();


            string s1 = ImageIOHelper.GetImageComment(filePath);
            Console.WriteLine(s1);

            if (s1 == "") {
                gameBoard = new GameBoard(bmp, gameMode, BORDER_WIDTH);

            } else {

                try {
                    SaveGame save = new SaveGame(s1);
                    timeOffset = save.TimeOffset;

                    gameBoard = new GameBoard(bmp, save, BORDER_WIDTH);

                } catch {
                    timeOffset = 0;
                    gameBoard = new GameBoard(bmp, gameMode, BORDER_WIDTH);
                }

            }

            gameBoard.FrameRate = frameRate;
            gameBoard.AnimationFunction = Functions.EaseInOut;

            RefreshImage();

            DrawBorders();

            gameBoardShadowImage.Source = gameBoard.GetShadowLayer();
            gameBoardShadowWithouthBordersImage.Source = gameBoard.GetShadowWithoutBordersLayer();



        }

        private void StartGame() {

            GAME_START_DATE = DateTime.Now;
            gameState = GAME_RUNNING;

            hasGameStarted = true;

        }

        public void RefreshImage() {

            if (MEASURE_BENCHMARK) {
                BenchmarkTimer.Start();
            }

            gameBoardImage.Source = gameBoard.GetGameBoardBitmapImage();

            if (MEASURE_BENCHMARK) {
                BenchmarkTimer.End();
                double seconds = BenchmarkTimer.GetSeconds();

                Console.WriteLine("" + seconds);
            } 
            
        }

        public void UpdateFrame() {

            if (gameBoard.HasFrameUpdate() || gameBoard.UpdateForBlur) {

                for (int i = 0; i < accelerateFactor; i++) {
                    gameBoard.UpdateFrame();
                }

                RefreshImage();

                gameBoard.UpdateForBlur = false;

                if (!gameBoard.HasFrameUpdate() &&
                    gameBoard.IsPuzzleSolved()) {
                    GameWon();
                }

            }

        }

        public void DismissWindow() {

            Application.Current.Shutdown();

        }

        private double windowAnimationStep = 0;

        public void FrameTick(object o, EventArgs sender) {

            if (windowSlideDirection == SLIDE_OUT) {

                int windowDismissYPosition = (int)SystemParameters.PrimaryScreenHeight;
                if (this.Top < windowDismissYPosition) {

                    this.Top += 35;

                    if (this.Top > windowDismissYPosition) {
                        this.Top = windowDismissYPosition;

                        windowSlideDirection = NONE;
                        DismissWindow();

                    }

                }

            } else if (windowSlideDirection == SLIDE_IN) {

                if (this.Top < windowYPositionCenter) {

                    double stepPerc = PuzzleGameLibrary.Functions.BounceFunction.Value(windowAnimationStep);
                    int totalDelta = (int)(SystemParameters.PrimaryScreenHeight / 2 + windowYPositionCenter);
                    double start = -SystemParameters.PrimaryScreenHeight / 2;
                    this.Top = start + stepPerc * totalDelta;

                    windowAnimationStep += 0.02;

                    if (this.Top >= windowYPositionCenter) {
                        this.Top = windowYPositionCenter;

                        if (!hasGameStarted) {

                            windowSlideDirection = NONE;
                            StartGame();

                        }

                    }

                }


            }


            if (gameBoard == null ||
                gameState != GAME_RUNNING) {
                return;
            }

            double seconds = (DateTime.Now - GAME_START_DATE).TotalSeconds + timeOffset;
            double percentage = seconds / (double)GAME_TIME_SECONDS;

            double secondsLeft = GAME_TIME_SECONDS - seconds;

            if (secondsLeft < 0) {
                // Game Over
                GameOver();
                return;
            }

            TimeSpan t = TimeSpan.FromSeconds(secondsLeft);

            string timeStr = string.Format("{0:D2}:{1:D2}",
                            t.Minutes,
                            t.Seconds);

            timerProgressLbl.Content = timeStr;


            timerProgress.Value = 100 - 100*percentage;

            // last 20 seconds of game
            if (secondsLeft < 20) {

                int blurRadius = 20 - (int)timerProgress.Value;

                gameBoard.LoadGausianBlur((int)Math.Pow(blurRadius, 1.43));

                BrushConverter bC = new BrushConverter();
                timerProgressLbl.Foreground = (Brush)bC.ConvertFrom("#FF9F0000");
     
            }

            UpdateFrame();

        }

        private void GameOver() {

            gameBoardSolutionImage.Source = (BitmapSource)gameOverSolutionBmpArray.GetBitmapImage();

            gameState = GAME_OVER;

            gameMessageLbl.Content = "Game Over!";

            FadeGameBoard();

            ImageIOHelper.AddImageComment(filePath, "");

            retryBtn.Visibility = Visibility.Visible;
            AnimationHelper.FadeObject(retryBtn, 0, 1, 1.0f);

        }

        private void GameWon() {

            gameState = GAME_WON;

            gameMessageLbl.Content = "You Win!";

            FadeGameBoard();

            ImageIOHelper.AddImageComment(filePath, "");

        }

        private void FadeGameBoard() {

            float duration = 1.0f;
            AnimationHelper.FadeObject(gameBoardImage, 1, 0, duration);
            AnimationHelper.FadeObject(bordersGrid, 1, 0, duration);
            AnimationHelper.FadeObject(gameBoardShadowImage, 1, 0, duration);
            AnimationHelper.FadeObject(gameBoardSolutionImage, 0, 1, duration);
            AnimationHelper.FadeObject(gameBoardShadowWithouthBordersImage, 0, 1, duration);
        }

        private void DrawBorders() {

            GeometryGroup bgGeoGroup = new GeometryGroup();
            Rect bgRect = new Rect(0, 0, bgGrid.ActualWidth, bgGrid.ActualHeight);

            bgGeoGroup.Children.Add(new RectangleGeometry(bgRect, 20, 20));

            bgGrid.Clip = bgGeoGroup;


            GeometryGroup gameBoardGeoGroup = new GeometryGroup();
            gameBoardGeoGroup.FillRule = FillRule.Nonzero;

            Point originWithPadding = PositionOfImage(gameBoardImage);
            Point origin = RealOriginInGameBoard();

            if (origin.X == 0 || origin.Y == 0) return;

            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();

            int border = gameBoard.BorderWidth;
            int paddingHori = (int)(origin.X - originWithPadding.X + border);
            int paddingVert = (int)(origin.Y - originWithPadding.Y + border);

            double fractionW = gameBoardImage.ActualWidth / (double)TILES_ROWS;
            double fractionH = gameBoardImage.ActualHeight / (double)TILES_COLLUMNS;

            for (int i = 0; i <= TILES_ROWS; i++) {

                Rect r = new Rect(originWithPadding.X - border / 2 + fractionW * i,
                0,
                paddingHori,
                bordersGrid.ActualHeight);

                gameBoardGeoGroup.Children.Add(new RectangleGeometry(r, 0, 0));

            }

            for (int i = 0; i <= TILES_COLLUMNS; i++) {

                Rect r = new Rect(0,
                originWithPadding.Y - border / 2 + fractionH * i,
                bordersGrid.ActualWidth,
                paddingVert
                );

                gameBoardGeoGroup.Children.Add(new RectangleGeometry(r, 0, 0));

            }

            bordersGrid.Clip = gameBoardGeoGroup;

        }

        private Point RealOriginInGameBoard() {

            int pBWidth = (int)gameBoardImage.ActualWidth;

            int pBHeight = (int)gameBoardImage.ActualHeight;

            if (pBWidth == 0 || pBHeight == 0) {
                return new Point(0, 0);
            }

            double pBRatio = pBWidth / (double)pBHeight;

            double imageRatio = gameBoard.Width / (double)gameBoard.Height;

            double imageToPbRatio;

            double imageWidth;
            double imageHeight;


            if (pBRatio > imageRatio) {

                // landscape
                imageHeight = pBHeight;

                imageToPbRatio = gameBoard.Height / (double)pBHeight;

                imageWidth = pBHeight * imageRatio;


            } else {
                // portrait
                imageWidth = pBWidth;

                imageToPbRatio = gameBoard.Width / (double)pBWidth;

                imageHeight = imageWidth / imageRatio;

            }

            double deltaX = imageWidth - pBWidth;
            double deltaY = imageHeight - pBHeight;

            Point pos = PositionOfImage(gameBoardImage);


            int x = (int)pos.X + (int)(Math.Abs(deltaX / 2) * (imageToPbRatio));
            int y = (int)pos.Y + (int)(Math.Abs(deltaY / 2) * (imageToPbRatio));

            return new Point(x, y);

        }

        private Point PositionOfImage(Image img) {

            Point p = img.TranslatePoint(new Point(0, 0), bordersGrid);
            double currentLeft = p.X;
            double currentTop = p.Y;
            return new Point(p.X, p.Y);

        }

        private Point PositionInImage(EventArgs e) {

            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.GetPosition(gameBoardImage);

            int pBWidth = (int)gameBoardImage.ActualWidth;

            int pBHeight = (int)gameBoardImage.ActualHeight;

            double pBRatio = pBWidth / (double)pBHeight;

            double imageRatio = gameBoard.Width / (double)gameBoard.Height;

            double imageToPbRatio;

            double imageWidth;
            double imageHeight;

            if (pBRatio > imageRatio) {

                // landscape
                imageHeight = pBHeight;

                imageToPbRatio = gameBoard.Height / (double)pBHeight;

                imageWidth = pBHeight * imageRatio;

            } else {
                // portrait
                imageWidth = pBWidth;

                imageToPbRatio = gameBoard.Width / (double)pBWidth;

                imageHeight = imageWidth / imageRatio;

            }

            double deltaX = imageWidth - pBWidth;
            double deltaY = imageHeight - pBHeight;

            int x = (int)((coordinates.X + deltaX / 2) * (imageToPbRatio));
            int y = (int)((coordinates.Y + deltaY / 2) * (imageToPbRatio));

            return new Point(x, y);

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e) {

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {

        }

        private void gameBoardImage_MouseUp(object sender, MouseButtonEventArgs e) {
            Point p = PositionInImage(e);
            gameBoard.BoardClicked((int)p.X, (int)p.Y);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            DrawBorders();
        }

        private void bgGrid_MouseDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        private void gameBoardImage_MouseMove(object sender, MouseEventArgs e) {

            Point p = PositionInImage(e);

            if (gameState == GAME_RUNNING &&
                gameBoard.IsValidClick((int)p.X, (int)p.Y)) {

                gameBoardImage.Cursor = Cursors.Hand;

            } else {

                gameBoardImage.Cursor = Cursors.Arrow;

            }

        }

        private void gameBoardImage_MouseDown(object sender, MouseButtonEventArgs e) {
            Point p = PositionInImage(e);

            if (gameState != GAME_RUNNING || 
                !gameBoard.IsValidClick((int)p.X, (int)p.Y)) {
                this.DragMove();
            }

        }

        private void UpdateFullscreenBtn() {
            var imageBrush = (ImageBrush)fullscreenBtn.Background;

            if (((App)(Application.Current)).FullScreen) {
                imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("exit_fullscreen_btn.png");
            } else {
                imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("fullscreen_btn.png");
            }
        }

        private void bordersGrid_MouseDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        private void saveBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)saveBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("save_btn_active.png");
        }

        private void saveBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)saveBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("save_btn.png");
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e) {
            int seconds = (int)((DateTime.Now - GAME_START_DATE).TotalSeconds + timeOffset);
            SaveGame save = new SaveGame(gameMode, seconds, gameBoard.Tiles);
            ImageIOHelper.AddImageComment(filePath, save.ToString());
        }

        private void newBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)newBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("new_game_btn_active.png");
        }

        private void newBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)newBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("new_game_btn.png");
        }

        private void newBtn_Click(object sender, RoutedEventArgs e) {

            SourcePickerWindow newWindow = new SourcePickerWindow();
            newWindow.Show();
            this.Close();

        }

        private void quitBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)quitBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("quit_btn_active.png");
        }

        private void quitBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)quitBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("quit_btn.png");
        }

        private void quitBtn_Click(object sender, RoutedEventArgs e) {

            if (FullScreen()) {

                DismissWindow();

            } else {

                windowSlideDirection = SLIDE_OUT;

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                gameBoard.MoveDown();
            } else if (e.Key == Key.Up) {
                gameBoard.MoveUp();
            } else if (e.Key == Key.Left) {
                gameBoard.MoveLeft();
            } else if (e.Key == Key.Right) {
                gameBoard.MoveRight();
            }
        }

        private void fullscreenBtn_Click(object sender, RoutedEventArgs e) {
            ((App)(Application.Current)).ToggleFullScreen(this);
            UpdateFullscreenBtn();
        }

        private void fullscreenBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {

            var imageBrush = (ImageBrush)fullscreenBtn.Background;

            if (((App)(Application.Current)).FullScreen) {
                imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("exit_fullscreen_btn_active.png");
            } else {
                imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("fullscreen_btn_active.png");
            }

        }

        private void fullscreenBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {

            var imageBrush = (ImageBrush)fullscreenBtn.Background;

            if (((App)(Application.Current)).FullScreen) {
                imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("exit_fullscreen_btn.png");
            } else {
                imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("fullscreen_btn.png");
            }

        }

        private void retryBtn_Click(object sender, RoutedEventArgs e) {
            GameModeSelectWindow newWindow = new GameModeSelectWindow(gameBmp, filePath); 
            newWindow.Show();
            this.Close();
        }

        private void retryBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)retryBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("retry_btn.png");
        }

        private void retryBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)retryBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("retry_btn_active.png");
        }

    }

}
