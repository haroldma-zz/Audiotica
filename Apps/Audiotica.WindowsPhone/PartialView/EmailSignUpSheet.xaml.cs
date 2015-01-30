using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Audiotica.Core.WinRt.Common;

namespace Audiotica.PartialView
{
    /// <summary>
    ///     Used to display a sheet that allows the user to sign up using an email address.
    /// </summary>
    public sealed partial class EmailSignUpSheet : IModalSheetPageAsync<bool>
    {
        private TaskCompletionSource<bool> emailTask;

        public EmailSignUpSheet()
        {
            InitializeComponent();
            App.SupressBackEvent += HardwareButtonsOnBackPressed;
        }

        public Task<bool> GetResultsAsync()
        {
            emailTask = new TaskCompletionSource<bool>();
            return emailTask.Task;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            App.SupressBackEvent -= HardwareButtonsOnBackPressed;
            emailTask.SetResult(false);
            emailTask = null;
        }

        private async void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            var username = UserBox.Text;
            var email = EmailBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                CurtainPrompt.ShowError("Looks like you forgot to fill out everything.");
            else
            {
                App.SupressBackEvent -= HardwareButtonsOnBackPressed;
                UiBlockerUtility.Block("Creating account...");

                var regResp = await App.Locator.AudioticaService.RegisterAsync(username, password, email);
                if (regResp.Success)
                {
                    emailTask.SetResult(true);
                    emailTask = null;
                }
                else
                {
                    App.SupressBackEvent += HardwareButtonsOnBackPressed;
                    CurtainPrompt.ShowError(regResp.Message ?? "Problem creating account.");
                }

                UiBlockerUtility.Unblock();
            }
        }

        public Popup Popup { get; private set; }

        public void OnClosed()
        {
        }

        public void OnOpened(Popup popup)
        {
            Popup = popup;
        }
    }
}