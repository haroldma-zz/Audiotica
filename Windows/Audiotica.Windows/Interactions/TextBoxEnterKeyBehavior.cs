using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Microsoft.Xaml.Interactivity;

namespace Audiotica.Windows.Interactions
{
    [ContentProperty(Name = "Actions")]
    [TypeConstraint(typeof (TextBox))]
    public class TextBoxEnterKeyBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register("Actions", typeof (ActionCollection),
                typeof (TextBoxEnterKeyBehavior), new PropertyMetadata(null));

        private TextBox AssociatedTextBox => AssociatedObject as TextBox;

        public ActionCollection Actions
        {
            get
            {
                var actions = (ActionCollection) GetValue(ActionsProperty);
                if (actions == null)
                {
                    SetValue(ActionsProperty, actions = new ActionCollection());
                }
                return actions;
            }
        }

        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;
            AssociatedTextBox.KeyDown += AssociatedTextBox_KeyDown;
        }

        public void Detach()
        {
            AssociatedTextBox.KeyDown -= AssociatedTextBox_KeyDown;
        }

        private void AssociatedTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Interaction.ExecuteActions(AssociatedObject, Actions, null);
            }
        }
    }
}