using System;
using System.Threading.Tasks;

namespace Audiotica.Core.Common
{
    public interface IConverter<T, TT> where TT : IConvertibleObject
    {
        Task<T> ConvertAsync(TT other, Action<TT> saveChanges = null);
    }

    public interface IConvertibleObject
    {
        object PreviousConversion { get; set; }
    }
}