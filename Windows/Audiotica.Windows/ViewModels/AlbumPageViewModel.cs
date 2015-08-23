using System.Collections.Generic;
using Audiotica.Database.Services.Interfaces;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class AlbumPageViewModel : ViewModelBase
    {
        private readonly ILibraryService _libraryService;
        private readonly List<IExtendedMetadataProvider> _metadataProviders;

        public AlbumPageViewModel(ILibraryService libraryService, IEnumerable<IMetadataProvider> metadataProviders)
        {
            _libraryService = libraryService;
            _metadataProviders = metadataProviders.FilterAndSort<IExtendedMetadataProvider>();
        }
    }
}