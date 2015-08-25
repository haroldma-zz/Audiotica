using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiotica.Core.Common
{
    public interface IConverter<T, TT> where T : IConvertibleObject
    {
        Task<T> FillPartialAsync(T other);
        Task<List<T>> FillPartialAsync(IEnumerable<T> others);
        Task<TT> ConvertAsync(T other, bool ignoreLibrary = false);
        Task<List<TT>> ConvertAsync(IEnumerable<T> others, bool ignoreLibrary = false);
    }

    public interface IConvertibleObject
    {
        object PreviousConversion { get; set; }
    }
}