namespace Audiotica.Web.MatchEngine.Validators
{
    public class SongLiveTypeValidator : BasicSongTypeValidatorBase
    {
        public override bool IsValid(string name, string matchName)
        {
            return IsTypeValid(name, matchName, "live", "concert", "arena");
        }
    }
}