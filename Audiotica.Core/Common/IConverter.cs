using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Audiotica.Core.Common
{
    public interface IConverter<in T, TT> where T : IConvertibleObject
    {
        Task<TT> ConvertAsync(T other);
        Task<List<TT>> ConvertAsync(IEnumerable<T> others);
    }

    public interface IConvertibleObject
    {
        object PreviousConversion { get; set; }
    }
}