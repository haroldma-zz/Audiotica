using Audiotica.Core.WinRt.Common;
using Audiotica.Data.Service.Interfaces;
using Audiotica.PartialView;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Audiotica.ViewModel
{
    public class AudioticaCloudViewModel : ViewModelBase
    {
        public AudioticaCloudViewModel(IAudioticaService service)
        {
            Service = service;
            SignInCommand = new RelayCommand(SignInExecute);
            SignUpCommand = new RelayCommand(SignUpExecute);
            SubscribeCommand = new RelayCommand(SubscribeExecute);
            LogoutCommand = new RelayCommand(LogoutExecute);
        }

        public RelayCommand LogoutCommand { get; set; }
        public IAudioticaService Service { get; private set; }
        public RelayCommand SignInCommand { get; set; }
        public RelayCommand SignUpCommand { get; set; }
        public RelayCommand SubscribeCommand { get; set; }

        private void LogoutExecute()
        {
            Service.Logout();
            CurtainPrompt.Show("Goodbye!");
        }

        private async void SignInExecute()
        {
            var signInSheet = new EmailSignInSheet();
            var success = await ModalSheetUtility.ShowAsync(signInSheet);

            if (success)
                CurtainPrompt.Show("Welcome!");
        }

        private async void SignUpExecute()
        {
            var signInSheet = new EmailSignUpSheet();
            var success = await ModalSheetUtility.ShowAsync(signInSheet);

            if (success)
                CurtainPrompt.Show("Welcome!");
        }

        private async void SubscribeExecute()
        {
            var subscribeSheet = new AudioticaSubscribeSheet();
            var success = await ModalSheetUtility.ShowAsync(subscribeSheet);

            if (success)
                CurtainPrompt.Show("Welcome to the cloud club!");
        }
    }
}