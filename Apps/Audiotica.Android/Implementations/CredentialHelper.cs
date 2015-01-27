using System;
using Audiotica.Android.Utilities;
using Audiotica.Core.Utils.Interfaces;

namespace Audiotica.Android.Implementations
{
    internal class CredentialHelper : ICredentialHelper
    {
        private readonly IAppSettingsHelper _appSettingsHelper;
        private const string BasePrefix = "AUTC_CREDENTIAL_";
        private const string UsernamePrefix = BasePrefix + "USERNAME_";
        private const string PasswordPrefix = BasePrefix + "PASSWORD_";
        private const string EncryptionKey = "NOTsoSECR3T_hunter2";

        public CredentialHelper(IAppSettingsHelper appSettingsHelper)
        {
            _appSettingsHelper = appSettingsHelper;
        }

        public IPasswordCredential GetCredentials(string resource)
        {
            var username = _appSettingsHelper.Read(UsernamePrefix + resource);
            var password =_appSettingsHelper.Read(PasswordPrefix + resource);

            if (!string.IsNullOrEmpty(password))
                password = Crypto.Decrypt(password, EncryptionKey);

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                return null;

            return new PasswordCredential(username, password);
        }

        public void SaveCredentials(string resource, string username, string password)
        {
            password = Crypto.Encrypt(password, EncryptionKey);
            _appSettingsHelper.Write(UsernamePrefix + resource, username);
            _appSettingsHelper.Write(PasswordPrefix + resource, password);
        }

        public void DeleteCredentials(string resource)
        {
            _appSettingsHelper.Write(UsernamePrefix + resource, null);
            _appSettingsHelper.Write(PasswordPrefix + resource, null);
        }
    }

    class PasswordCredential : IPasswordCredential
    {
        private readonly string _username;
        private readonly string _password;

        public PasswordCredential(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public string GetUsername()
        {
            return _username;
        }

        public string GetPassword()
        {
            return _password;
        }
    }
}