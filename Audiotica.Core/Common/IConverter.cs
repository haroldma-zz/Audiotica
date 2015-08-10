using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiotica.Core.Common
{
    public interface IConverter<T, TT> where T : IConvertibleObject
    {
        Task<TT> ConvertAsync(T other, Action<T> saveChanges = null);
        Task<List<TT>> ConvertAsync(IEnumerable<T> others);
    }

    public interface IConvertibleObject
    {
        object PreviousConversion { get; set; }
    }
}