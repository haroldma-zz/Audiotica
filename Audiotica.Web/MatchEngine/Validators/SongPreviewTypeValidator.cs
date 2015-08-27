namespace Audiotica.Web.MatchEngine.Validators
{
    public class SongPreviewTypeValidator : BasicSongTypeValidatorBase
    {
        public override bool IsValid(string name, string matchName)
        {
            return IsTypeValid(name, matchName, "preview", "snipped");
        }
    }
}