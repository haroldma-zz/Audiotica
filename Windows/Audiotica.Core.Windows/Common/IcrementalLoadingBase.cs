using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Audiotica.Core.Windows.Common
{
    public abstract class IncrementalLoadingBase<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        #region State 

        public bool Busy { get; private set; }

        #endregion

        #region Private methods 

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                var items = await LoadMoreItemsOverrideAsync(c, count);

                if (items == null)
                    return new LoadMoreItemsResult();

                foreach (var item in items)
                    Add(item);

                return new LoadMoreItemsResult {Count = (uint) items.Count};
            }
            finally
            {
                Busy = false;
            }
        }

        #endregion

        #region ISupportIncrementalLoading 

        public bool HasMoreItems => HasMoreItemsOverride();

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (Busy)
            {
                throw new InvalidOperationException("Only one operation in flight at a time");
            }

            Busy = true;

            return AsyncInfo.Run(c => LoadMoreItemsAsync(c, count));
        }

        #endregion

        #region Overridable methods 

        protected abstract Task<IList<T>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count);
        protected abstract bool HasMoreItemsOverride();

        #endregion
    }
}