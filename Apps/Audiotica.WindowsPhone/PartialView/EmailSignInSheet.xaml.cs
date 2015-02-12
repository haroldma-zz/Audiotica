#region

using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Audiotica.Core.WinRt.Common;

#endregion

namespace Audiotica.PartialView
{
    /// <summary>
    ///     Used to display a sheet that allows the user to sign in using an email address.
    /// </summary>
    public sealed partial class EmailSignInSheet : IModalSheetPageAsync<bool>
    {
        private TaskCompletionSource<bool> emailTask;

        public EmailSignInSheet()
        {
            InitializeComponent();
            App.SupressBackEvent += HardwareButtonsOnBackPressed;
        }

        public Task<bool> GetResultsAsync()
        {
            emailTask = new TaskCompletionSource<bool>();
            return emailTask.Task;
        }

        public Popup Popup { get; private set; }

        public void OnClosed()
        {
        }

        public void OnOpened(Popup popup)
        {
            Popup = popup;
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
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                CurtainPrompt.ShowError("Looks like you forgot your email/password.");
            else
            {
                App.SupressBackEvent -= HardwareButtonsOnBackPressed;
                UiBlockerUtility.Block("Signing in...");

                var regResp = await App.Locator.AudioticaService.LoginAsync(username, password);
                if (regResp.Success)
                {
                    emailTask.SetResult(true);
                    emailTask = null;
                }
                else
                {
                    App.SupressBackEvent += HardwareButtonsOnBackPressed;
                    CurtainPrompt.ShowError(regResp.Message ?? "Problem signing you in.");
                }

                UiBlockerUtility.Unblock();
            }
        }
    }
}