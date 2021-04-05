#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using MahApps.Metro.Controls;

#endregion

namespace RabaMetroStyle.Navigation
{
    public class FrameAnimator
    {
        public static readonly DependencyProperty FrameNavigationStoryboardProperty
            = DependencyProperty.RegisterAttached(
                "FrameNavigationStoryboard",
                typeof(Storyboard),
                typeof(FrameAnimator),
                new FrameworkPropertyMetadata(null, FrameNavigationStoryboardPropertyChanged));

        private static void FrameNavigationStoryboardPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Frame frame && (e.OldValue != e.NewValue))
            {
                frame.Navigating -= Frame_Navigating;
                if (e.NewValue is Storyboard)
                {
                    frame.Navigating += Frame_Navigating;
                }
            }
        }

        public static Storyboard GetFrameNavigationStoryboard(DependencyObject control)
        {
            return control.GetValue(FrameNavigationStoryboardProperty) as Storyboard;
        }

        public static void SetFrameNavigationStoryboard(DependencyObject control, Storyboard storyboard)
        {
            control.SetValue(FrameNavigationStoryboardProperty, storyboard);
        }

        private static void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (sender is Frame frame)
            {
                var sb = GetFrameNavigationStoryboard(frame);
                if (sb != null)
                {
                    var presenter = frame.FindChild<ContentPresenter>();
                    sb.Begin((FrameworkElement)presenter ?? frame);
                }
            }
        }
    }
}