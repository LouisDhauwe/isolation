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
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window {
        public SplashWindow() : base() {

            // show splash screen 2.5 seconds extra
            System.Threading.Thread.Sleep(2500);

            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {

            SourcePickerWindow newWindow = new SourcePickerWindow();
            newWindow.Show();
            this.Close();

        }

    }
}
