using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LangFromTextWinApp.Helpers
{
    public class AnimSuccesFail
    {
        private FrameworkElement guiElement;
        ScaleTransform myScaleTransform;
        DoubleAnimation scaleDoubleAnimation;
        ColorAnimation colorAnimation;
        SolidColorBrush backgroundBrush;

        public AnimSuccesFail(FrameworkElement guiElement, double durationMiliseconds, bool autoReverse)
        {
            this.guiElement = guiElement;

            backgroundBrush = new SolidColorBrush(Colors.Transparent);

            if (this.guiElement is Panel)
                (this.guiElement as Panel).Background = backgroundBrush;
            else
                (this.guiElement as Control).Background = backgroundBrush;

            colorAnimation = new ColorAnimation(Colors.Transparent, new Duration(TimeSpan.FromMilliseconds(durationMiliseconds)));
            colorAnimation.AutoReverse = autoReverse;

            scaleDoubleAnimation = new DoubleAnimation(1, 1.1, new Duration(TimeSpan.FromMilliseconds(durationMiliseconds - (durationMiliseconds / 10))));
            scaleDoubleAnimation.AutoReverse = autoReverse;

            myScaleTransform = new ScaleTransform();

            this.guiElement.RenderTransform = new TransformGroup { Children = { myScaleTransform } };
        }

        public void AnimSuccess()
        {
            myScaleTransform.CenterX = guiElement.ActualWidth / 2;
            myScaleTransform.CenterY = guiElement.ActualHeight / 2;

            colorAnimation.To = Color.FromArgb(40, 0x0, 0xFF, 0x0);

            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            myScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDoubleAnimation);
            myScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDoubleAnimation);
        }

        public void AnimFail()
        {
            myScaleTransform.CenterX = guiElement.ActualWidth / 2;
            myScaleTransform.CenterY = guiElement.ActualHeight / 2;

            colorAnimation.To = Color.FromArgb(40, 0xFF, 0x0, 0x0);

            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            myScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleDoubleAnimation);
            myScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleDoubleAnimation);
        }
    }
}
