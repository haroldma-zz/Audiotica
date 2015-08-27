using System;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Audiotica.Windows.Interactions
{
    /// <summary>
    ///     An action that switches the current visual to the specified <see cref="T:Windows.UI.Xaml.Controls.Page" />.
    /// </summary>
    public sealed class NavigateToPageAction : DependencyObject, IAction
    {
        /// <summary>
        ///     Identifies the <seealso cref="P:Microsoft.Xaml.Interactions.Core.NavigateToPageAction.TargetPage" /> dependency
        ///     property.
        /// </summary>
        public static readonly DependencyProperty TargetPageProperty = DependencyProperty.Register("TargetPage",
            typeof (Type), typeof (NavigateToPageAction), new PropertyMetadata(null));

        /// <summary>
        ///     Identifies the <seealso cref="P:Microsoft.Xaml.Interactions.Core.NavigateToPageAction.Parameter" /> dependency
        ///     property.
        /// </summary>
        public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register("Parameter",
            typeof (object), typeof (NavigateToPageAction), new PropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the fully qualified name of the <see cref="T:Windows.UI.Xaml.Controls.Page" /> to navigate to. This is
        ///     a dependency property.
        /// </summary>
        public Type TargetPage
        {
            get { return (Type) GetValue(TargetPageProperty); }
            set { SetValue(TargetPageProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the parameter which will be passed to the
        ///     <see cref="M:Windows.UI.Xaml.Controls.Frame.Navigate(System.Type,System.Object)" /> method.
        /// </summary>
        public object Parameter
        {
            get { return GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        /// <summary>
        ///     Executes the action.
        /// </summary>
        /// <param name="sender">
        ///     The <see cref="T:System.Object" /> that is passed to the action by the behavior. Generally this is
        ///     <seealso cref="P:Microsoft.Xaml.Interactivity.IBehavior.AssociatedObject" /> or a target object.
        /// </param>
        /// <param name="parameter">The value of this parameter is determined by the caller.</param>
        /// <returns>
        ///     True if the navigation to the specified page is successful; else false.
        /// </returns>
        public object Execute(object sender, object parameter)
        {
            return TargetPage != null && App.Current.NavigationService.Navigate(TargetPage, parameter.ToString());
        }
    }
}