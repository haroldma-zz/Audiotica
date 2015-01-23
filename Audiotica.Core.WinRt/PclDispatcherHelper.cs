#region

using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Audiotica.Core.Utils.Interfaces;

#endregion

namespace Audiotica.Core.WinRt
{
    public class PclDispatcherHelper : IDispatcherHelper
    {
        private readonly CoreDispatcher _dispatcher;

        public PclDispatcherHelper(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task RunAsync(Action action)
        {
            await _dispatcher
                .RunAsync(CoreDispatcherPriority.Normal,
                    new DispatchedHandler(action));
        }
    }
}