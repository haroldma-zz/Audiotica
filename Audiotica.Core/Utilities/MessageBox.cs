#region

using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;

#endregion

namespace Audiotica.Core.Utilities
{
    public static class MessageBox
    {
        public static async Task<MessageBoxResult> ShowAsync(string messageBoxText,
            string caption,
            MessageBoxButton button = MessageBoxButton.Ok)
        {
            var result = MessageBoxResult.None;
            var md = new MessageDialog(messageBoxText, caption);

            if (button.HasFlag(MessageBoxButton.Ok))
            {
                md.Commands.Add(new UICommand("OK",
                    cmd => result = MessageBoxResult.Ok));
            }
            if (button.HasFlag(MessageBoxButton.Yes))
            {
                md.Commands.Add(new UICommand("Yes",
                    cmd => result = MessageBoxResult.Yes));
            }
            if (button.HasFlag(MessageBoxButton.No))
            {
                md.Commands.Add(new UICommand("No",
                    cmd => result = MessageBoxResult.No));
            }
            if (button.HasFlag(MessageBoxButton.Cancel))
            {
                md.Commands.Add(new UICommand("Cancel",
                    cmd => result = MessageBoxResult.Cancel));
                md.CancelCommandIndex = (uint) md.Commands.Count - 1;
            }
            try
            {
                await md.ShowAsync();
            }
            catch { }
            return result;
        }

        public static async Task<MessageBoxResult> ShowAsync(string messageBoxText)
        {
            return await ShowAsync(messageBoxText, null);
        }

        public static async void Show(string messageBoxText)
        {
            await ShowAsync(messageBoxText, "");
        }

        public static async void Show(string messageBoxText, string messageBoxTitle, MessageBoxButton button = MessageBoxButton.Ok)
        {
            await ShowAsync(messageBoxText, messageBoxTitle, button);
        }
    }

    // Summary:
    //     Specifies the buttons to include when you display a message box.
    [Flags]
    public enum MessageBoxButton
    {
        // Summary:
        //     Displays only the OK button.
        Ok = 1,
        // Summary:
        //     Displays only the Cancel button.
        Cancel = 2,
        //
        // Summary:
        //     Displays both the OK and Cancel buttons.
        OkCancel = Ok | Cancel,
        // Summary:
        //     Displays only the OK button.
        Yes = 4,
        // Summary:
        //     Displays only the Cancel button.
        No = 8,
        //
        // Summary:
        //     Displays both the OK and Cancel buttons.
        YesNo = Yes | No,
    }

    // Summary:
    //     Represents a user's response to a message box.
    public enum MessageBoxResult
    {
        // Summary:
        //     This value is not currently used.
        None = 0,
        //
        // Summary:
        //     The user clicked the OK button.
        Ok = 1,
        //
        // Summary:
        //     The user clicked the Cancel button or pressed ESC.
        Cancel = 2,
        //
        // Summary:
        //     This value is not currently used.
        Yes = 6,
        //
        // Summary:
        //     This value is not currently used.
        No = 7,
    }
}