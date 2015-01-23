using Windows.Security.Credentials;
using Audiotica.Core.Utils.Interfaces;

namespace Audiotica.Core.WinRt
{
    internal class PclPasswordCredential : IPasswordCredential
    {
        public PasswordCredential Credential { get; private set; }

        public PclPasswordCredential(PasswordCredential credential)
        {
            Credential = credential;
            Credential.RetrievePassword();
        }

        public string GetUsername()
        {
            return Credential.UserName;
        }

        public string GetPassword()
        {
            return Credential.Password;
        }
    }
}