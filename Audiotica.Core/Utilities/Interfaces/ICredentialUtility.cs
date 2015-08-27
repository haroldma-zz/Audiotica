namespace Audiotica.Core.Utilities.Interfaces
{
    public interface ICredentialUtility
    {
        AppCredential GetCredentials(string resource);
        void SaveCredentials(string resource, string username, string password);
        void DeleteCredentials(string resource);
    }

    public class AppCredential
    {
        public AppCredential(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }
        public string Password { get; }
    }
}