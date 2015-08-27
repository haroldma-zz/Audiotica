using System.Linq;
using Windows.Security.Credentials;
using Audiotica.Core.Utilities.Interfaces;

namespace Audiotica.Core.Windows.Utilities
{
    public class CredentialUtility : ICredentialUtility
    {
        private readonly PasswordVault _vault;

        public CredentialUtility()
        {
            _vault = new PasswordVault();
        }

        public AppCredential GetCredentials(string resource)
        {
            var credential = InternalGet(resource);
            if (credential == null) return null;
            credential.RetrievePassword();
            return new AppCredential(credential.UserName, credential.Password);
        }

        public void SaveCredentials(string resource, string username, string password)
        {
            // Create and store the user credentials.
            var credential = new PasswordCredential(resource,
                username, password);
            _vault.Add(credential);
        }

        public void DeleteCredentials(string resource)
        {
            var credential = InternalGet(resource);
            if (credential != null)
                _vault.Remove(credential);
        }

        private PasswordCredential InternalGet(string resource)
        {
            return _vault.RetrieveAll().FirstOrDefault(p => p.Resource == resource);
        }
    }
}