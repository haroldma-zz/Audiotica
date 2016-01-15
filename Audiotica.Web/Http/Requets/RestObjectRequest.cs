using System.Threading.Tasks;
using Audiotica.Core.Extensions;
using Audiotica.Web.Extensions;

namespace Audiotica.Web.Http.Requets
{
    public abstract class RestObjectRequest<T> : RestRequest
    {
        public virtual async Task<RestResponse<T>> ToResponseAsync()
        {
            return await this.Fetch<T>().DontMarshall();
        }
    }
}