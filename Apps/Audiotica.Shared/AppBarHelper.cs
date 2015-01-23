#region

using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Audiotica.Core.Utilities;
using Audiotica.Core.Utils;

#endregion

namespace Audiotica
{
    public static class AppBarHelper
    {
        private static List<ICommandBarElement> _originalCommands;
        private static List<ICommandBarElement> _originalSecondaryCommands;

        public static void SaveState(CommandBar appBar)
        {
            _originalCommands = appBar.PrimaryCommands.ToList();
            _originalSecondaryCommands = appBar.SecondaryCommands.ToList();
        }

        public static void RestorePreviousState(CommandBar appBar)
        {
            if (_originalCommands != null)
            {
                appBar.PrimaryCommands.Clear();
                appBar.PrimaryCommands.AddRange(_originalCommands);
            }

            if (_originalSecondaryCommands != null)
            {
                appBar.SecondaryCommands.Clear();
                appBar.SecondaryCommands.AddRange(_originalSecondaryCommands);
            }
        }

        public static void SwitchState(CommandBar appBar, IEnumerable<ICommandBarElement> primary,
            IEnumerable<ICommandBarElement> secondary = null)
        {
            appBar.PrimaryCommands.Clear();
            appBar.SecondaryCommands.Clear();

            if (primary != null)
                appBar.PrimaryCommands.AddRange(primary);

            if (secondary != null)
                appBar.SecondaryCommands.AddRange(secondary);
        }
    }
}