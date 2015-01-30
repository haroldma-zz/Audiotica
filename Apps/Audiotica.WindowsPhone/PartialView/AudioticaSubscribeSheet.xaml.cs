#region

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Audiotica.Core.WinRt.Common;
using Audiotica.Data.Model;
using Audiotica.Data.Model.AudioticaCloud;

#endregion

namespace Audiotica.PartialView
{
    public sealed partial class AudioticaSubscribeSheet : IModalSheetPageAsync<bool>
    {
        private TaskCompletionSource<bool> billingTask;

        public AudioticaSubscribeSheet()
        {
            InitializeComponent();
            var months = new string[12];

            for (var i = 0; i < 12; i++)
            {
                months[i] = (i+1).ToString("D2");
            }

            ExpMonthBox.ItemsSource = months;

            var years = new string[20];
            var currentYear = DateTime.Now.Year;

            for (var i = 0; i < 20; i++)
            {
                years[i] = (currentYear + i).ToString();
            }

            ExpYearBox.ItemsSource = years;

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

        private bool isBackKey;
        private void CardNumBox_TextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            if (isBackKey)
                return;

            var nums = CardNumBox.Text;
            nums = Regex.Replace(nums, "[^0-9]", "");

            var divs = (int)Math.Floor(nums.Length/4.0);
            var placedDivs = 0;

            for (var i = 1; i <= divs; i++)
            {
                var index = i*4 + placedDivs;
                if (i == divs && index == nums.Length)
                    continue;

                nums = nums.Insert(index, "-");
                placedDivs++;
            }

            CardNumBox.Text = nums;
            CardNumBox.Select(CardNumBox.Text.Length, 0);
        }

        private void CardNumBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            isBackKey = e.Key == VirtualKey.Back;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var card = new AudioticaStripeCard()
            {
                Name = CardNameBox.Text,
                Number = CardNumBox.Text,
                Cvc = CardSecurityCode.Text
            };

            var nameEmpty = string.IsNullOrEmpty(card.Name);
            var numberEmpty = string.IsNullOrEmpty(card.Number);
            var cvcEmpty = string.IsNullOrEmpty(card.Cvc);
            var expEmpty = ExpMonthBox.SelectedIndex == -1 || ExpYearBox.SelectedIndex == -1;

            if (nameEmpty && numberEmpty && cvcEmpty && expEmpty)
            {
                CurtainPrompt.ShowError("You forgot to enter all your card information.");
            }

            else if (nameEmpty)
            {
                CurtainPrompt.ShowError("You forgot to enter the name on the card.");
            }

            else if (numberEmpty)
            {
                CurtainPrompt.ShowError("You forgot to enter the card's number.");
            }

            else if (cvcEmpty)
            {
                CurtainPrompt.ShowError("You forgot to enter the card's security code.");
            }

            else if (expEmpty)
            {
                CurtainPrompt.ShowError("You forgot to enter the card's expiration date.");
            }

            else
            {
                card.ExpMonth = int.Parse(ExpMonthBox.SelectedItem as string);
                card.ExpYear = int.Parse(ExpYearBox.SelectedItem as string);

                var term = SubcriptionTimeFrame.Month;

                switch (PlanBox.SelectedIndex)
                {
                    case 1:
                        term = SubcriptionTimeFrame.Biyear;
                        break;
                    case 2:
                        term = SubcriptionTimeFrame.Year;
                        break;
                }

                UiBlockerUtility.Block("Subscribing...");
                var result = await App.Locator.AudioticaService.SubscribeAsync(SubscriptionType.Silver, term, card);
                UiBlockerUtility.Unblock();

                if (result.Success)
                {
                    billingTask.SetResult(true);
                }
                else
                {
                    CurtainPrompt.ShowError(result.Message);
                }
            }
        }
    }
}