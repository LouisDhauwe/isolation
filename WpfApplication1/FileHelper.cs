using Microsoft.Win32;
using PuzzleGameLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PuzzleGame {
    class FileHelper {

        public static Bitmap LoadAndProcessBitmap(ref string filePath, Size maxSize) {

            string path = "";

            path = GetImagePathFromPrompt();

            if (path == null) {
                return null;
            }

            filePath = path;

            Bitmap bmp = new System.Drawing.Bitmap(path);

            Size newSize = ImageStreamHelper.ResizeKeepAspect(bmp.Size, maxSize.Width, maxSize.Height);

            bmp = ImageStreamHelper.ResizeImage(bmp, newSize);

            return bmp;
        }

        public static string GetImagePathFromPrompt() {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Image files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png";
            bool? didSelect = openFile.ShowDialog();
            if (didSelect == false) {
                return null;
            }

            return openFile.FileName;
        }

        public static BitmapImage GetResourceBitmapImage(string res) {
            var filename = @"pack://application:,,,/Resources/" + res;
            return new BitmapImage(new Uri(filename));
        }

    }
}
