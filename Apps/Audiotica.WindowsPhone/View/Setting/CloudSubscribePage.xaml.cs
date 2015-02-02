using System;
using System.Text.RegularExpressions;

using Audiotica.Core.WinRt.Common;
using Audiotica.Data.Model;
using Audiotica.Data.Model.AudioticaCloud;

using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Audiotica.View.Setting
{
    public sealed partial class CloudSubscribePage
    {
        private bool _isBackKey;

        public CloudSubscribePage()
        {
            this.InitializeComponent();
            var months = new string[12];

            for (var i = 0; i < 12; i++)
            {
                months[i] = (i + 1).ToString("D2");
            }

            this.ExpMonthBox.ItemsSource = months;

            var years = new string[20];
            var currentYear = DateTime.Now.Year;

            for (var i = 0; i < 20; i++)
            {
                years[i] = (currentYear + i).ToString();
            }

            this.ExpYearBox.ItemsSource = years;
        }

        private async void ButtonClick(object sender, RoutedEventArgs e)
        {
            var card = new AudioticaStripeCard
            {
                Name = this.CardNameBox.Text, 
                Number = this.CardNumBox.Text, 
                Cvc = this.CardSecurityCode.Text
            };

            var nameEmpty = string.IsNullOrEmpty(card.Name);
            var numberEmpty = string.IsNullOrEmpty(card.Number);
            var cvcEmpty = string.IsNullOrEmpty(card.Cvc);
            var expEmpty = this.ExpMonthBox.SelectedIndex == -1 || this.ExpYearBox.SelectedIndex == -1;

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
                card.ExpMonth = int.Parse(this.ExpMonthBox.SelectedItem as string);
                card.ExpYear = int.Parse(this.ExpYearBox.SelectedItem as string);

                var term = SubcriptionTimeFrame.Month;

                switch (this.PlanBox.SelectedIndex)
                {
                    case 1:
                        term = SubcriptionTimeFrame.Biyear;
                        break;
                    case 2:
                        term = SubcriptionTimeFrame.Year;
                        break;
                }

                UiBlockerUtility.Block("Subscribing...");
                var result =
                    await
                    App.Locator.AudioticaService.SubscribeAsync(
                        SubscriptionType.Silver, 
                        term, 
                        card, 
                        this.CouponCode.Text.Trim());
                UiBlockerUtility.Unblock();

                if (result.Success)
                {
                    CurtainPrompt.Show("Welcome to the Cloud Club!");
                    App.Navigator.GoBack();
                }
                else
                {
                    CurtainPrompt.ShowError(result.Message);
                }
            }
        }

        private void CardNumBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            this._isBackKey = e.Key == VirtualKey.Back;
        }

        private void CardNumBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._isBackKey || string.IsNullOrEmpty(this.CardNumBox.Text))
            {
                return;
            }

            var nums = this.CardNumBox.Text;
            nums = Regex.Replace(nums, "[^0-9]", string.Empty);

            var divs = (int)Math.Floor(nums.Length / 4.0);
            var placedDivs = 0;

            for (var i = 1; i <= divs; i++)
            {
                var index = (i * 4) + placedDivs;
                if (i == divs && index == nums.Length)
                {
                    continue;
                }

                nums = nums.Insert(index, "-");
                placedDivs++;
            }

            this.CardNumBox.Text = nums;

            if (!string.IsNullOrEmpty(nums))
            {
                this.CardNumBox.Select(nums.Length, 0);
            }
        }
    }
}