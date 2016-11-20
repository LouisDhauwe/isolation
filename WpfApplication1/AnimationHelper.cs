using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PuzzleGame {
    
    public class AnimationHelper {

        public static void FadeObject(DependencyObject obj, float from, float to, float duration) {

            Storyboard sb = new Storyboard();
            DoubleAnimation dA = new DoubleAnimation();

            dA.From = from;
            dA.To = to;
            dA.Duration = new Duration(TimeSpan.FromSeconds(duration));
            sb.Children.Add(dA);
            Storyboard.SetTargetProperty(dA, new PropertyPath(UserControl.OpacityProperty));
            Storyboard.SetTarget(dA, obj);
            sb.Begin();

        }

    
    }
}
