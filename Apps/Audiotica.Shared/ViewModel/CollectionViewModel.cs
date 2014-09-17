#region

using Audiotica.Collection;
using GalaSoft.MvvmLight;

#endregion

namespace Audiotica.ViewModel
{
    public class CollectionViewModel : ViewModelBase
    {
        private readonly ICollectionService _service;

        public CollectionViewModel(ICollectionService service)
        {
            _service = service;

            if (IsInDesignModeStatic)
                _service.LoadLibrary();
        }

        public ICollectionService Service
        {
            get { return _service; }
        }
    }
}