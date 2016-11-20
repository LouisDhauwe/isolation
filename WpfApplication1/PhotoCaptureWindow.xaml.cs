using PuzzleGameLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
    /// Interaction logic for PhotoCaptureWindow.xaml
    /// </summary>
    public partial class PhotoCaptureWindow : Window {
        public PhotoCaptureWindow() : base() {
            InitializeComponent();

            if (((App)Application.Current).FullScreen) {
                ((App)Application.Current).MakeFullScreen(this);
            }

            UpdateFullscreenBtn();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            capPlayer.Dispose();
        }

        private void fullscreenBtn_Click(object sender, RoutedEventArgs e) {
            ((App)(Application.Current)).ToggleFullScreen(this);
            UpdateFullscreenBtn();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            ClipViews();
        }

        private void bgGrid_MouseDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        private async void captureBtn_Click(object sender, RoutedEventArgs e) {

            if (((App)Application.Current).FullScreen) {
                flashView.Opacity = 1.0;
                await Task.Delay(200);
            }

            System.Windows.Media.ImageSource src = capPlayer.Source;

            Bitmap bmp = new Bitmap(ImageStreamHelper.ImageWpfToGDI(src));
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipXY);

            System.Drawing.Size newSize = ImageStreamHelper.ResizeKeepAspect(bmp.Size, 300, 300);

            bmp = ImageStreamHelper.ResizeImage(bmp, newSize);
            bmp = ImageStreamHelper.ResizeImageToSquare(bmp);

            if (((App)Application.Current).FullScreen) {
                await Task.Delay(200);
            }

            GameModeSelectWindow newWindow = new GameModeSelectWindow(bmp, "");
            newWindow.Show();
            this.Close();

        }

        private void ClipViews() {

            // bottom bar
            borders.Opacity = 1.0f;
            GeometryGroup overlayGroup = new GeometryGroup();
            overlayGroup.FillRule = FillRule.Nonzero;
            Rect overlayRect = new Rect(0, borders.ActualHeight-130, borders.ActualWidth, 130);

            // removes rounded corners
            Rect overlayRect2 = new Rect(0, borders.ActualHeight - 130, borders.ActualWidth, 40);

            overlayGroup.Children.Add(new RectangleGeometry(overlayRect, 20, 20));
            overlayGroup.Children.Add(new RectangleGeometry(overlayRect2, 0, 0));

            borders.Clip = overlayGroup;
            
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

        private void captureBtn_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)captureBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("capture_photo_btn.png");
        }

        private void captureBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var imageBrush = (ImageBrush)captureBtn.Background;
            imageBrush.ImageSource = FileHelper.GetResourceBitmapImage("capture_photo_btn_active.png");
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
