using System.Linq;
using Windows.Security.Credentials;
using Audiotica.Core.Utils.Interfaces;

namespace Audiotica.Core.WinRt.Utilities
{
    public class PclCredentialHelper : ICredentialHelper
    {
        public IPasswordCredential GetCredentials(string resource)
        {
            var vault = new PasswordVault();
            
            var credential = vault.RetrieveAll().FirstOrDefault(p => p.Resource == resource);
            return credential == null 
                ? null 
                : new PclPasswordCredential(credential);
        }

        public void SaveCredentials(string resource, string username, string password)
        {
            // Create and store the user credentials.
            var credential = new PasswordCredential(resource,
                username, password);
            new PasswordVault().Add(credential);
        }

        public void DeleteCredentials(string resource)
        {
            var credential = GetCredentials(resource) as PclPasswordCredential;
            if (credential != null)
                new PasswordVault().Remove(credential.Credential);
        }
    }
}
