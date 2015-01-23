namespace Audiotica.Core.Utils.Interfaces
{
    public interface IPasswordCredential
    {
        string GetUsername();
        string GetPassword();
    }

    public interface ICredentialHelper
    {
        IPasswordCredential GetCredentials(string resource);

        void SaveCredentials(string resource, string username, string password);

        void DeleteCredentials(string resource);
    }
}
