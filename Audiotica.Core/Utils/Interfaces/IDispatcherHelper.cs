#region

using System;
using System.Threading.Tasks;

#endregion

namespace Audiotica.Core.Utils.Interfaces
{
    public interface IDispatcherHelper
    {
        Task RunAsync(Action action);
    }
}