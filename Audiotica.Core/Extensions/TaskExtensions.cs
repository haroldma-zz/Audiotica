using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Audiotica.Core.Extensions
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable<TResult> DontMarshall<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }
    }
}