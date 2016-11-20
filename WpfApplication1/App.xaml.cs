using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfApplication1 {

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        private bool _fullScreen = false;

        public bool FullScreen {
            get { return _fullScreen; }
        }

        public void ToggleFullScreen(Window window) {

            if (_fullScreen) {

                window.WindowState = WindowState.Normal;
                window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            } else {

                MakeFullScreen(window);

            }

            _fullScreen = !_fullScreen;

        }

        public void MakeFullScreen(Window window) {

            window.WindowState = WindowState.Maximized;
            window.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));

        }

    }

}
