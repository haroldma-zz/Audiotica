using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace Audiotica.Core.Utilities
{
    public static class CredentialHelper
    {
        public static PasswordCredential GetCredentials(string resource)
        {
            var vault = new PasswordVault();
            PasswordCredential credential = null;

            try
            {
                // Try to get an existing credential from the vault.
                credential = vault.FindAllByResource(resource).FirstOrDefault();
            }
            catch (Exception)
            {
                // When there is no matching resource an error occurs, which we ignore.
            }

            if (credential == null) return null;

            credential.RetrievePassword();
            return credential;
        }

        public static void SaveCredentials(string resource, string username, string password)
        {
            // Create and store the user credentials.
            var credential = new PasswordCredential(resource,
                username, password);
            new PasswordVault().Add(credential);
        }

        public static void DeleteCredentials(string resource)
        {
            new PasswordVault().Remove(GetCredentials(resource));
        }
    }
}
