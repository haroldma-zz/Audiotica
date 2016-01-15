using System;
using Audiotica.Web.Extensions;

namespace Audiotica.Web.Http.Requets.Metadata.Google
{
    internal class MusixMatchGetTokenRequest : MusixMatchBaseRequest
    {
        public MusixMatchGetTokenRequest() : base("token.get")
        {
            this.QParam("guid", Guid.NewGuid().ToString());
        }
    }
}