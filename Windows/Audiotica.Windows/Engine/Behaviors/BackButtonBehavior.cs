using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Audiotica.Core.Utilities.Interfaces;
using Microsoft.Xaml.Interactivity;

namespace Audiotica.Windows.Engine.Behaviors
{
    [ContentProperty(Name = nameof(Actions))]
    public class BackButtonBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register(nameof(Actions),
                typeof (ActionCollection),
                typeof (TextBoxEnterKeyBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty HandledProperty =
            DependencyProperty.Register(nameof(Handled),
                typeof (bool),
                typeof (BackButtonBehavior),
                new PropertyMetadata(false));

        private IDispatcherUtility _dispatcher;

        public ActionCollection Actions
        {
            get
            {
                var actions = (ActionCollection)GetValue(ActionsProperty);
                if (actions == null)
                {
                    SetValue(ActionsProperty, actions = new ActionCollection());
                }
                return actions;
            }
        }

        public DependencyObject AssociatedObject { get; set; }

        public bool Handled
        {
            get
            {
                return (bool)GetValue(HandledProperty);
            }
            set
            {
                SetValue(HandledProperty, value);
            }
        }

        public void Attach(DependencyObject associatedObject)
        {
            _dispatcher = WindowWrapper.Current().Dispatcher;
            BootStrapper.BackRequested += BootStrapper_BackRequested;
        }

        public void Detach()
        {
            BootStrapper.BackRequested -= BootStrapper_BackRequested;
        }

        private void BootStrapper_BackRequested(object sender, HandledEventArgs e)
        {
            e.Handled = Handled;
            foreach (IAction item in Actions)
            {
                _dispatcher.Run(() => item.Execute(sender, null));
            }
        }
    }
}