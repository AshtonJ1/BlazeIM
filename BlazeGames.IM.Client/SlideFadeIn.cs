using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BlazeGames.IM.Client
{
    internal class SlideFade
    {
        static int control = 0;

        public static void StartAnimationIn(FrameworkElement wnd)
        {
            Storyboard myStoryboard = new Storyboard();

            foreach (Object obj in LogicalTreeHelper.GetChildren(wnd))
            {
                try
                {
                    UIElement element = (UIElement)obj;
                    Timeline[] Animations = CreateAnimationIn(element);
                    myStoryboard.Children.Add(Animations[0]);
                    myStoryboard.Children.Add(Animations[1]);
                }
                catch { }
            }

            myStoryboard.Begin(wnd);
            control = 0;
        }

        private static Timeline[] CreateAnimationIn(UIElement obj)
        {
            //  Create and set the translation.
            var translation = new TranslateTransform() { X = -40.00, Y = 0 };
            obj.RenderTransform = translation;

            //  Create an animation for the opacity.
            var opacityAnimation = new DoubleAnimation() { From = 0, To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(750)) };

            //  Create an animation for the slide in.
            var slideInAnimation = new DoubleAnimation() { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(750)) };
            slideInAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };

            //  Set the targets for the animations.
            Storyboard.SetTarget(opacityAnimation, obj);
            Storyboard.SetTarget(slideInAnimation, obj);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTargetProperty(slideInAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            //  Return the animations.
            control++;
            return new Timeline[] { opacityAnimation, slideInAnimation };
        }

        public static void CreateAnimationInMinimum(UIElement obj)
        {
            //  Create and set the translation.
            var translation = new TranslateTransform() { X = -40.00, Y = 0 };
            obj.RenderTransform = translation;

            //  Create an animation for the opacity.
            var opacityAnimation = new DoubleAnimation() { From = 0, To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(750)) };

            //  Create an animation for the slide in.
            var slideInAnimation = new DoubleAnimation() { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(750)) };
            slideInAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };

            //  Set the targets for the animations.
            Storyboard.SetTarget(opacityAnimation, obj);
            Storyboard.SetTarget(slideInAnimation, obj);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTargetProperty(slideInAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            Storyboard sb = new Storyboard();
            sb.Children.Add(slideInAnimation);
            sb.Children.Add(opacityAnimation);
            sb.Begin();
        }
        private static double LastLeftPosition = 0;
        private static double LastTopPosition = 0;
        private static Storyboard sb_tominimum;
        public static void CreateAnimationToMinimum()
        {
            LastLeftPosition = MainWindow.Instance.Left;
            LastTopPosition = MainWindow.Instance.Top;
            //  Create an animation for the slide in.
            var slideInAnimation = new DoubleAnimation() { From = MainWindow.Instance.Width, To = 70, Duration = new Duration(TimeSpan.FromMilliseconds(1000)) };
            slideInAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            var slideOverAnimation = new DoubleAnimation() { From = MainWindow.Instance.Left, To = SystemParameters.PrimaryScreenWidth - 70, Duration = new Duration(TimeSpan.FromMilliseconds(1000)) };
            slideOverAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            var slideUpAnimation = new DoubleAnimation() { From = MainWindow.Instance.Top, To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(1000)) };
            slideUpAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            var slideHeightAnimation = new DoubleAnimation() { From = MainWindow.Instance.Height, To = SystemParameters.WorkArea.Height, Duration = new Duration(TimeSpan.FromMilliseconds(1000)) };
            slideHeightAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            //  Set the targets for the animations.
            Storyboard.SetTarget(slideInAnimation, MainWindow.Instance);
            Storyboard.SetTargetProperty(slideInAnimation, new PropertyPath(MainWindow.WindowWidthAnimationProperty));
            Storyboard.SetTarget(slideHeightAnimation, MainWindow.Instance);
            Storyboard.SetTargetProperty(slideHeightAnimation, new PropertyPath(MainWindow.WindowHeightAnimationProperty));
            Storyboard.SetTarget(slideUpAnimation, MainWindow.Instance);
            Storyboard.SetTargetProperty(slideUpAnimation, new PropertyPath("Top"));
            Storyboard.SetTarget(slideOverAnimation, MainWindow.Instance);
            Storyboard.SetTargetProperty(slideOverAnimation, new PropertyPath("Left"));

            sb_tominimum = new Storyboard();
            sb_tominimum.Children.Add(slideInAnimation);
            sb_tominimum.Children.Add(slideOverAnimation);
            sb_tominimum.Children.Add(slideUpAnimation);
            sb_tominimum.Children.Add(slideHeightAnimation);

            MainWindow.Instance.txt_search.Visibility = Visibility.Hidden;
            MainWindow.Instance.txt_debug.Visibility = Visibility.Hidden;
            MainWindow.Instance.nav_bar.Visibility = Visibility.Hidden;

            sb_tominimum.Completed += new EventHandler(sb_tominimum_Completed);

            if (sb_tominimum_max != null)
                sb_tominimum_max.Stop();
            if (sb_tominimum_min != null)
                sb_tominimum_min.Stop();

            sb_tominimum.Begin();
        }

        static void sb_tominimum_Completed(object sender, EventArgs e)
        {
            MainWindow.Instance.Minimum = true;
            if (!MainWindow.Instance.IsMouseOver)
                CreateAnimationToMinimum_min();
        }

        private static Storyboard sb_tomaximum;
        public static void CreateAnimationToMaximum()
        {
            //  Create an animation for the slide in.
            var slideInAnimation = new DoubleAnimation() { From = MainWindow.Instance.Width, To = 850, Duration = new Duration(TimeSpan.FromMilliseconds(1000)) };
            slideInAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            var slideOverAnimation = new DoubleAnimation() { From = MainWindow.Instance.Left, To = LastLeftPosition, Duration = new Duration(TimeSpan.FromMilliseconds(1000)) };
            slideOverAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            var slideDownAnimation = new DoubleAnimation() { From = MainWindow.Instance.Top, To = LastTopPosition, Duration = new Duration(TimeSpan.FromMilliseconds(1000)) };
            slideDownAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
            var slideHeightAnimation = new DoubleAnimation() { From = MainWindow.Instance.Height, To = 575, Duration = new Duration(TimeSpan.FromMilliseconds(1000)) };
            slideHeightAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            //  Set the targets for the animations.
            Storyboard.SetTarget(slideInAnimation, MainWindow.Instance);
            Storyboard.SetTargetProperty(slideInAnimation, new PropertyPath(MainWindow.WindowWidthAnimationProperty));
            Storyboard.SetTarget(slideHeightAnimation, MainWindow.Instance);
            Storyboard.SetTargetProperty(slideHeightAnimation, new PropertyPath(MainWindow.WindowHeightAnimationProperty));
            Storyboard.SetTarget(slideDownAnimation, MainWindow.Instance);
            Storyboard.SetTargetProperty(slideDownAnimation, new PropertyPath("Top"));
            Storyboard.SetTarget(slideOverAnimation, MainWindow.Instance);
            Storyboard.SetTargetProperty(slideOverAnimation, new PropertyPath("Left"));

            sb_tomaximum = new Storyboard();
            sb_tomaximum.Children.Add(slideInAnimation);
            sb_tomaximum.Children.Add(slideOverAnimation);
            sb_tomaximum.Children.Add(slideDownAnimation);
            sb_tomaximum.Children.Add(slideHeightAnimation);

            sb_tomaximum.Completed += new EventHandler(sb_tomaximum_Completed);

            if (sb_tominimum_max != null)
                sb_tominimum_max.Stop();
            if (sb_tominimum_min != null)
                sb_tominimum_min.Stop();

            sb_tomaximum.Begin();
        }


        private static Storyboard sb_tominimum_min;
        public static void CreateAnimationToMinimum_min()
        {
            //if (sb_tominimum_min == null)
            //{
                //  Create an animation for the slide in.
                var slideInAnimation = new DoubleAnimation() { To = 3, Duration = new Duration(TimeSpan.FromMilliseconds(800)), BeginTime = TimeSpan.FromSeconds(1) };
                slideInAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
                var slideOverAnimation = new DoubleAnimation() { To = SystemParameters.PrimaryScreenWidth - 3, Duration = new Duration(TimeSpan.FromMilliseconds(800)), BeginTime = TimeSpan.FromSeconds(1) };
                slideOverAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

                //  Set the targets for the animations.
                Storyboard.SetTarget(slideInAnimation, MainWindow.Instance);
                Storyboard.SetTargetProperty(slideInAnimation, new PropertyPath("Width"));
                Storyboard.SetTarget(slideOverAnimation, MainWindow.Instance);
                Storyboard.SetTargetProperty(slideOverAnimation, new PropertyPath("Left"));

                sb_tominimum_min = new Storyboard();
                sb_tominimum_min.Children.Add(slideInAnimation);
                sb_tominimum_min.Children.Add(slideOverAnimation);
            //}

            if(sb_tominimum_max != null)
                sb_tominimum_max.Stop();

            sb_tominimum_min.Begin();
        }

        private static Storyboard sb_tominimum_max;
        public static void CreateAnimationToMinimum_max()
        {
            //if (sb_tominimum_max == null)
            //{
                //  Create an animation for the slide in.
                var slideInAnimation = new DoubleAnimation() { To = 70, Duration = new Duration(TimeSpan.FromMilliseconds(800)), BeginTime = TimeSpan.FromMilliseconds(200) };
                slideInAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };
                var slideOverAnimation = new DoubleAnimation() { To = SystemParameters.PrimaryScreenWidth - 70, Duration = new Duration(TimeSpan.FromMilliseconds(800)), BeginTime = TimeSpan.FromMilliseconds(200) };
                slideOverAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

                //  Set the targets for the animations.
                Storyboard.SetTarget(slideInAnimation, MainWindow.Instance);
                Storyboard.SetTargetProperty(slideInAnimation, new PropertyPath("Width"));
                Storyboard.SetTarget(slideOverAnimation, MainWindow.Instance);
                Storyboard.SetTargetProperty(slideOverAnimation, new PropertyPath("Left"));

                sb_tominimum_max = new Storyboard();
                sb_tominimum_max.Children.Add(slideInAnimation);
                sb_tominimum_max.Children.Add(slideOverAnimation);
            //}

            if(sb_tominimum_min != null)
                sb_tominimum_min.Stop();

            sb_tominimum_max.Begin();
        }

        static void sb_tomaximum_Completed(object sender, EventArgs e)
        {
            MainWindow.Instance.Width = 850;

            MainWindow.Instance.txt_search.Visibility = Visibility.Visible;

            if (ConfigManager.Instance.GetBool("indev", false))
                MainWindow.Instance.txt_debug.Visibility = Visibility.Visible;

            MainWindow.Instance.nav_bar.Visibility = Visibility.Visible;
        }

        public static Storyboard sb_profileimg = new Storyboard();
        public static bool SlideOut = false;

        public static void CreateProfileImgAnimationIn(UIElement obj)
        {
            if (!SlideOut)
                return;

            //  Create and set the translation.
            var translation = new TranslateTransform() { X = 100.00, Y = 0 };
            obj.RenderTransform = translation;

            //  Create an animation for the opacity.
            var opacityAnimation = new DoubleAnimation() { From = 0, To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(750)) };

            //  Create an animation for the slide in.
            var slideInAnimation = new DoubleAnimation() { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(750)) };
            slideInAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            //  Set the targets for the animations.
            Storyboard.SetTarget(opacityAnimation, obj);
            Storyboard.SetTarget(slideInAnimation, obj);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTargetProperty(slideInAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            sb_profileimg.Stop();
            sb_profileimg.Children.Clear();

            sb_profileimg.Children.Add(opacityAnimation);
            sb_profileimg.Children.Add(slideInAnimation);
            sb_profileimg.Begin();
            SlideOut = false;
        }

        public static void CreateProfileImgAnimationOut(UIElement obj)
        {
            if (SlideOut)
                return;

            //  Create and set the translation.
            var translation = new TranslateTransform() { X = 0, Y = 0 };
            obj.RenderTransform = translation;

            //  Create an animation for the opacity.
            var opacityAnimation = new DoubleAnimation() { From = 1, To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(750)) };

            //  Create an animation for the slide in.
            var slideInAnimation = new DoubleAnimation() { To = 100, Duration = new Duration(TimeSpan.FromMilliseconds(750)) };
            slideInAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            //  Set the targets for the animations.
            Storyboard.SetTarget(opacityAnimation, obj);
            Storyboard.SetTarget(slideInAnimation, obj);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTargetProperty(slideInAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            sb_profileimg.Stop();
            sb_profileimg.Children.Clear();

            sb_profileimg.Children.Add(opacityAnimation);
            sb_profileimg.Children.Add(slideInAnimation);
            sb_profileimg.Begin();
            SlideOut = true;
        }

        public static void CancelProfileImgAnimation1()
        {
            sb_profileimg.Stop();
            SlideOut = !SlideOut;
        }

        public static void CancelProfileImgAnimation2()
        {
            sb_profileimg.Stop();
        }
    }
}
