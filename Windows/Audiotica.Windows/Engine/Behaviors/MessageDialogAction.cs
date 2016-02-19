using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Audiotica.Windows.Engine.Behaviors
{
    public class MessageDialogAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var d = new MessageDialog(Content, Title);
            Task.Run(async () => await d.ShowAsync());
            return this;
        }

        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(string),
                typeof(MessageDialogAction), new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string),
                typeof(MessageDialogAction), new PropertyMetadata(string.Empty));

    }
}
