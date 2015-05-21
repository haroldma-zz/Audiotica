using Audiotica.Web.MatchEngine.Providers;

namespace Audiotica.Web.Interfaces.MatchEngine.Validators
{
    /// <summary>
    ///     Utilized by the <see cref="ProviderBase.VerifyMatchesAsync"/> helper to validated a match's type. 
    /// </summary>
    public interface ISongTypeValidator
    {
        bool IsValid(string name, string matchName);
    }
}