using System.Threading.Tasks;

using Audiotica.Data.Collection.Model;

using GalaSoft.MvvmLight.Messaging;

using Windows.Phone.UI.Input;

namespace Audiotica.View
{
    public sealed partial class ManualMatchPage
    {
        private TaskCompletionSource<string> task;

        public ManualMatchPage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += this.HardwareButtonsBackPressed;
        }

        public override void NavigatedTo(Windows.UI.Xaml.Navigation.NavigationMode mode, object parameter)
        {
            base.NavigatedTo(mode, parameter);
            var song = parameter as Song;
            Messenger.Default.Send(song, "manual-match");
        }

        public Task<string> WaitForMatchAsync()
        {
            this.task = new TaskCompletionSource<string>();
            return this.task.Task;
        }

        private void HardwareButtonsBackPressed(object sender, BackPressedEventArgs e)
        {
            if (this.task != null)
            {
                this.task.SetResult(null);
            }
        }
    }
}