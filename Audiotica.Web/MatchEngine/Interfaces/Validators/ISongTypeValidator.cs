using Audiotica.Web.MatchEngine.Providers;

namespace Audiotica.Web.MatchEngine.Interfaces.Validators
{
    /// <summary>
    ///     Utilized by the <see cref="MatchProviderBase.VerifyMatchesAsync" /> helper to validated a match's type.
    /// </summary>
    public interface ISongTypeValidator
    {
        bool IsValid(string name, string matchName);
    }
}