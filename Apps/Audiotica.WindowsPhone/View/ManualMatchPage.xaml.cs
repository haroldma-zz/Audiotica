using System.Threading.Tasks;

using Audiotica.Data.Collection.Model;

using GalaSoft.MvvmLight.Messaging;

using Windows.Phone.UI.Input;

namespace Audiotica.View
{
    public sealed partial class ManualMatchPage
    {
        public ManualMatchPage()
        {
            this.InitializeComponent();
        }

        public override void NavigatedTo(Windows.UI.Xaml.Navigation.NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            var song = parameter as Song;
            Messenger.Default.Send(song, "manual-match");
        }
    }
}