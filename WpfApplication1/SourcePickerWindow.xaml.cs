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
using PuzzleGameLibrary.Model;

namespace PuzzleGame {
    /// <summary>
    /// Interaction logic for SourcePickerWindow.xaml
    /// </summary>
    public partial class SourcePickerWindow : Window {
        public SourcePickerWindow() : base() {
            InitializeComponent();

            if (((App)Application.Current).FullScreen) {
                ((App)Application.Current).MakeFullScreen(this);
            }

            UpdateFullscreenBtn();

        }

        private void fileSourceBtn_Click(object sender, RoutedEventArgs e) {

            string filePath = "";
            Bitmap bmp = FileHelper.LoadAndProcessBitmap(ref filePath, new System.Drawing.Size(300, 300));

            if (bmp == null) {
                return;
            }

            string metadata = ImageIOHelper.GetImageComment(filePath);

            Window newWindow = null;

            if (metadata != "") {

                try {
                    SaveGame save = new SaveGame(metadata);
                    newWindow = new MainWindow(bmp, filePath, save.GameMode);
                } catch {
                    newWindow = null;
                }

            }

            if (newWindow == null) {
                newWindow = new GameModeSelectWindow(bmp, filePath);
            }

            newWindow.Show();
            this.Close();

        }

        private void cameraSourceBtn_Click(object sender, RoutedEventArgs e) {
            PhotoCaptureWindow newWindow = new PhotoCaptureWindow();
            newWindow.Show();
            this.Close();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        }

        private void fullscreenBtn_Click(object sender, RoutedEventArgs e) {
            ((App)(Application.Current)).ToggleFullScreen(this);

            UpdateFullscreenBtn();

        }

        private void UpdateFullscreenBtn() {
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

            GeometryGroup bgGeoGroup = new GeometryGroup();
            Rect bgRect = new Rect(0, 0, bgGrid.ActualWidth, bgGrid.ActualHeight);

            bgGeoGroup.Children.Add(new RectangleGeometry(bgRect, 20, 20));
            bgGrid.Clip = bgGeoGroup;

        }

        private void cameraSourceBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)cameraSourceBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("take_photo_btn_active.png");
        }

        private void cameraSourceBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)cameraSourceBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("take_photo_btn.png");
        }

        private void fileSourceBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)fileSourceBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("pick_file_btn_active.png");
        }

        private void fileSourceBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)fileSourceBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("pick_file_btn.png");
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

    }
}
