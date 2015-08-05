using System.Threading.Tasks;

namespace Audiotica.Factory
{
    public interface IConverter<T, in TT>
    {
        Task<T> ConvertAsync(TT other);
    }
}