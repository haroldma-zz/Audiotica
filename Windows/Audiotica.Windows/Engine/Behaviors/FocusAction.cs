using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace Audiotica.Windows.Engine.Behaviors
{
    public class FocusAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var ui = sender as Control;
            if (ui != null)
                ui.Focus(FocusState.Programmatic);
            return null;
        }
    }
}
