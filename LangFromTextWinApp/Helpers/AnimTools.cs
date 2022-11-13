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
    public static class AnimTools
    {
        public static void FadeAnim(this FrameworkElement guiElement, double from, double to, double durationMiliseconds = 1000)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(durationMiliseconds - (durationMiliseconds / 10))));

            guiElement.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        }

        public static void ToBackgroundAnim(this FrameworkElement guiElement, Color toColor, double durationMiliseconds = 1000)
        {
            ColorAnimation colorAnimation = new ColorAnimation(toColor, new Duration(TimeSpan.FromMilliseconds(durationMiliseconds)));

            SolidColorBrush backgroundBrush = new SolidColorBrush(Colors.Transparent);

            if (guiElement is Panel)
                (guiElement as Panel).Background = backgroundBrush;
            else
                (guiElement as Control).Background = backgroundBrush;

            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        public static void AnimTranslateX(this FrameworkElement guiElement, double from, double to, IEasingFunction easingFunction, double durationMiliseconds = 1000)
        {
            TranslateTransform tTrans = new TranslateTransform();
            guiElement.RenderTransform = tTrans;

            DoubleAnimation doubleAnimation = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(durationMiliseconds)));
            doubleAnimation.EasingFunction = easingFunction;

            tTrans.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
        }

        public static void AnimTranslateY(this FrameworkElement guiElement, double from, double to, IEasingFunction easingFunction, double durationMiliseconds = 1000)
        {
            TranslateTransform tTrans = new TranslateTransform();
            guiElement.RenderTransform = tTrans;

            DoubleAnimation doubleAnimation = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(durationMiliseconds)));
            doubleAnimation.EasingFunction = easingFunction;

            tTrans.BeginAnimation(TranslateTransform.YProperty, doubleAnimation);
        }
    }
}
