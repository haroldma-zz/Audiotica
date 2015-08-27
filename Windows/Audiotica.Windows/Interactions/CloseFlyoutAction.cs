using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace Audiotica.Windows.Interactions
{
    public sealed class CloseFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            try
            {
                var popups = VisualTreeHelper.GetOpenPopups(Window.Current);
                if (popups != null)
                {
                    if (popups.Any())
                    {
                        var popup = popups.FirstOrDefault(x => x.Child.GetType() == typeof (FlyoutPresenter));
                        if (popup != null)
                        {
                            popup.IsOpen = false;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }
    }
}