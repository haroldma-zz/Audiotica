using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.Views
{
    public sealed partial class ShellPhone
    {
        // back
        private Command _backCommand;
        // menu
        private Command _menuCommand;
        // nav
        private Command<NavType> _navCommand;

        public ShellPhone(Frame frame)
        {
            InitializeComponent();
            ShellSplitView.Content = frame;
            var update = new Action(() =>
            {
                // update radiobuttons after frame navigates
                var type = frame.CurrentSourcePageType;
                foreach (var radioButton in AllRadioButtons(this))
                {
                    var target = radioButton.CommandParameter as NavType;
                    if (target == null)
                        continue;
                    radioButton.IsChecked = target.Type == type;
                }
                ShellSplitView.IsPaneOpen = false;
                BackCommand.RaiseCanExecuteChanged();
            });
            frame.Navigated += (s, e) => update();
            Loaded += (s, e) => update();
            DataContext = this;
        }

        public Command BackCommand => _backCommand ?? (_backCommand = new Command(ExecuteBack, CanBack));
        public Command MenuCommand => _menuCommand ?? (_menuCommand = new Command(ExecuteMenu));
        public Command<NavType> NavCommand => _navCommand ?? (_navCommand = new Command<NavType>(ExecuteNav));

        public bool CanBack()
        {
            var nav = App.Current.NavigationService;
            return nav.CanGoBack;
        }

        private static void ExecuteBack()
        {
            var nav = App.Current.NavigationService;
            nav.GoBack();
        }

        private void ExecuteMenu()
        {
            ShellSplitView.IsPaneOpen = !ShellSplitView.IsPaneOpen;
        }

        public void ExecuteNav(NavType navType)
        {
            var type = navType.Type;
            var nav = ((App) Application.Current).NavigationService;

            // when we nav home, clear history
            if (type == typeof (ExplorePage))
                nav.ClearHistory();

            // navigate only to new pages
            if (nav.CurrentPageType != null && nav.CurrentPageType != type)
                nav.Navigate(type, navType.Parameter);
        }

        // utility
        public List<RadioButton> AllRadioButtons(DependencyObject parent)
        {
            var list = new List<RadioButton>();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is RadioButton)
                {
                    list.Add(child as RadioButton);
                    continue;
                }
                list.AddRange(AllRadioButtons(child));
            }
            return list;
        }

        // prevent check
        private void DontCheck(object s, RoutedEventArgs e)
        {
            // don't let the radiobutton check
            ((RadioButton) s).IsChecked = false;
        }
    }
}