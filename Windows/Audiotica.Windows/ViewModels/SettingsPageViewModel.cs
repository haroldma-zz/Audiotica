using System.Linq;
using Windows.Storage;
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
        }

        public IAppSettingsUtility AppSettingsUtility { get; }

        public DelegateCommand DeleteCommand { get; set; }

        public DelegateCommand ImportCommand { get; set; }

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
    }
}