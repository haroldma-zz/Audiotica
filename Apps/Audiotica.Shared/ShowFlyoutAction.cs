#region

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Xaml.Interactivity;

#endregion

namespace Audiotica
{
    public class ShowFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                FlyoutBase.ShowAttachedFlyout(element);
            }

            return sender;
        }
    }
}