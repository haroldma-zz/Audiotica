using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace Audiotica.Windows.Engine.Behaviors
{
    public class CloseFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var parent = sender as DependencyObject;
            while (parent != null)
            {
                if (parent is FlyoutPresenter)
                {
                    ((parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
                    break;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
            return null;
        }
    }
}
