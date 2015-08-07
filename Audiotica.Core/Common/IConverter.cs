using System;
using System.Threading.Tasks;

namespace Audiotica.Core.Common
{
    public interface IConverter<T, TT> where T : IConvertibleObject
    {
        Task<TT> ConvertAsync(T other, Action<T> saveChanges = null);
    }

    public interface IConvertibleObject
    {
        object PreviousConversion { get; set; }
    }
}