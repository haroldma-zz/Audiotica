using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Database.Models;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Windows.Common;
using Audiotica.Windows.Engine.Mvvm;
using Audiotica.Windows.Services.Interfaces;

namespace Audiotica.Windows.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        private readonly ILibraryService _libraryService;
        private readonly IMusicImportService _musicImportService;
        private bool _canToggleAds;

        public SettingsPageViewModel(
            IAppSettingsUtility appSettingsUtility,
            IMusicImportService musicImportService,
            ILibraryService libraryService)
        {
            _musicImportService = musicImportService;
            _libraryService = libraryService;
            AppSettingsUtility = appSettingsUtility;

            ImportCommand = new DelegateCommand(ImportExecute);
            DeleteCommand = new DelegateCommand(DeleteExecute);
            PurchaseAdCommand = new DelegateCommand(PurchaseAdExecute);
        }

        public IAppSettingsUtility AppSettingsUtility { get; }

        public bool CanToggleAds
        {
            get
            {
                return _canToggleAds;
            }
            set
            {
                Set(ref _canToggleAds, value);
            }
        }

        public DelegateCommand DeleteCommand { get; set; }

        public DelegateCommand ImportCommand { get; set; }

        public DelegateCommand PurchaseAdCommand { get; }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            AnalyticService.TrackPageView("Settings");
            CanToggleAds = App.Current.LicenseInformation.ProductLicenses["InAppAds"].IsActive;
        }

        private async void DeleteExecute()
        {
            using (UiBlocker.Show("Deleting..."))
            {
                foreach (var track in _libraryService.Tracks.Where(p => p.Type == TrackType.Local).ToList())
                {
                    await _libraryService.DeleteTrackAsync(track);
                }
            }
        }

        private async void ImportExecute()
        {
            using (var blocker = UiBlocker.Show("Scanning..."))
            {
                var files = await _musicImportService.ScanFolderAsync(KnownFolders.MusicLibrary);
                blocker.UpdateProgress($"Importing {files.Count}...");

                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];

                    blocker.UpdateProgress($"Importing {i + 1}/{files.Count}...");

                    try
                    {
                        await _musicImportService.SaveAsync(file);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private async void PurchaseAdExecute()
        {
            if (App.Current.LicenseInformation.ProductLicenses["InAppAds"].IsActive)
                return;

            try
            {
                // Show the purchase dialog
#if DEBUG
                await CurrentAppSimulator.RequestProductPurchaseAsync("InAppAds", false).AsTask();
#else
                await CurrentApp.RequestProductPurchaseAsync("InAppAds", false).AsTask();
#endif
                CanToggleAds = true;
            }
            catch
            {
                CurtainPrompt.ShowError("Purchase wasn't succesful");
            }
        }
    }
}