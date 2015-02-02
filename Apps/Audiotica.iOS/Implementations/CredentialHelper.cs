using Audiotica.Core.Utils.Interfaces;

using Foundation;

using Security;

namespace Audiotica.iOS.Implementations
{
    internal class CredentialHelper : ICredentialHelper
    {
        public IPasswordCredential GetCredentials(string resource)
        {
            var rec = this.GetRecord(resource);
            return rec != null ? new PasswordCredential(rec.Account, rec.ValueData.ToString()) : null;
        }

        public void SaveCredentials(string resource, string username, string password)
        {
            this.DeleteCredentials(resource);

            var s = new SecRecord(SecKind.GenericPassword)
            {
                Label = string.Format("Audiotica Credential ({0})", resource), 
                Description = "Credentials from the Audiotica app", 
                Account = username, 
                Service = "Audiotica", 
                ValueData = NSData.FromString(password), 
                Generic = NSData.FromString(resource)
            };
            SecKeyChain.Add(s);
        }

        public void DeleteCredentials(string resource)
        {
            var record = this.GetRecord(resource);
            if (record != null)
            {
                SecKeyChain.Remove(record);
            }
        }

        private SecRecord GetRecord(string resource)
        {
            var rec = new SecRecord(SecKind.GenericPassword) { Generic = NSData.FromString(resource) };

            SecStatusCode res;
            return SecKeyChain.QueryAsRecord(rec, out res);
        }
    }

    internal class PasswordCredential : IPasswordCredential
    {
        private readonly string _password;

        private readonly string _username;

        public PasswordCredential(string username, string password)
        {
            this._username = username;
            this._password = password;
        }

        public string GetUsername()
        {
            return this._username;
        }

        public string GetPassword()
        {
            return this._password;
        }
    }
}