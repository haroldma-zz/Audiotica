#region

using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Audiotica.Core.WinRt.Common;

#endregion

namespace Audiotica.PartialView
{
    public sealed partial class AudioticaSubscribeSheet : IModalSheetPageAsync<bool>
    {
        private TaskCompletionSource<bool> billingTask;

        public AudioticaSubscribeSheet()
        {
            InitializeComponent();
            App.SupressBackEvent += HardwareButtonsOnBackPressed;
        }

        public Task<bool> GetResultsAsync()
        {
            billingTask = new TaskCompletionSource<bool>();
            return billingTask.Task;
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
            billingTask.SetResult(false);
            billingTask = null;
        }
    }
}