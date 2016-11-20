using PuzzleGameLibrary.Helpers;
using PuzzleGameLibrary.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApplication1;

namespace PuzzleGame {
    /// <summary>
    /// Interaction logic for GameModeSelectWindow.xaml
    /// </summary>
    public partial class GameModeSelectWindow : Window {

        private Bitmap bmp;
        private string filePath;

        public GameModeSelectWindow(Bitmap bmp, string filePath) {

            InitializeComponent();

            if (((App)Application.Current).FullScreen) {
                ((App)Application.Current).MakeFullScreen(this);
            }

            UpdateFullscreenBtn();

            this.bmp = bmp;
            this.filePath = filePath;

            BitmapSource source = ImageStreamHelper.ToBitmapSource(bmp);
            gameBoardImage.Source = source;

            if (FullScreen()) {
                flashView.Opacity = 1.0f;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            if (FullScreen()) {

                AnimationHelper.FadeObject(flashView, 1, 0, 0.5f);
                await Task.Delay(500);
            }
        }

        private bool FullScreen() {
            return ((App)Application.Current).FullScreen;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            ClipViews();
        }

        private void bgGrid_MouseDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        private void ClipViews() {

            // round window corners
            GeometryGroup bgGeoGroup = new GeometryGroup();
            Rect bgRect = new Rect(0, 0, bgGrid.ActualWidth, bgGrid.ActualHeight);

            bgGeoGroup.Children.Add(new RectangleGeometry(bgRect, 20, 20));

            bgGrid.Clip = bgGeoGroup;

        }

        private void UpdateFullscreenBtn() {
            var imageBrush = (ImageBrush)fullscreenBtn.Background;

            if (((App)(Application.Current)).FullScreen) {
                imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("exit_fullscreen_btn.png");
            } else {
                imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("fullscreen_btn.png");
            }
        }

        private void easyBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)easyBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("easy_btn.png");
        }

        private void easyBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)easyBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("easy_btn_active.png");
        }

        private void normalBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)normalBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("normal_btn.png");
        }

        private void normalBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)normalBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("normal_btn_active.png");
        }

        private void hardBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)hardBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("hard_btn_active.png");
        }

        private void hardBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)hardBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("hard_btn.png");
        }

        private void easyBtn_Click(object sender, RoutedEventArgs e) {
            ShowGameWindow(GameModesData.GameModes.EASY);
        }

        private void normalBtn_Click(object sender, RoutedEventArgs e) {
            ShowGameWindow(GameModesData.GameModes.NORMAL);
        }

        private void hardBtn_Click(object sender, RoutedEventArgs e) {
            ShowGameWindow(GameModesData.GameModes.HARD);
        }

        private void ShowGameWindow(GameModesData.GameModes gameMode) {
            MainWindow newWindow = new MainWindow(bmp, filePath, gameMode);
            newWindow.Show();
            this.Close();
        }

    }
}
