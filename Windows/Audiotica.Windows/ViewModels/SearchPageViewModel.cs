using System.Collections.Generic;
using Audiotica.Web.Extensions;
using Audiotica.Web.Metadata.Interfaces;
using Audiotica.Windows.Tools.Mvvm;

namespace Audiotica.Windows.ViewModels
{
    public class SearchPageViewModel : ViewModelBase
    {
        private List<ISearchMetadataProvider> _searchProviders;
        private ISearchMetadataProvider _selectedSearchProvider;

        public SearchPageViewModel(IEnumerable<IMetadataProvider> metadataProviders)
        {
            _searchProviders = metadataProviders.FilterAndSort<ISearchMetadataProvider>();
            SelectedSearchProvider = _searchProviders[0];
        }

        public ISearchMetadataProvider SelectedSearchProvider
        {
            get { return _selectedSearchProvider; }
            set { Set(ref _selectedSearchProvider, value); }
        }

        public List<ISearchMetadataProvider> SearchProviders
        {
            get { return _searchProviders; }
            set { Set(ref _searchProviders, value); }
        }
    }
}