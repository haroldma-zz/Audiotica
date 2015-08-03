using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Core.Windows.Enums
{
    /// <summary>
    /// Indicates the state of the background task.
    /// </summary>
    /// <remarks>
    /// State is persisted to the application settings store so that the foreground
    /// process can discover and respond to the current state of the background task.
    /// </remarks>
    public enum BackgroundTaskState
    {
        Unknown,
        Started,
        Running,
        Canceled
    }
}
