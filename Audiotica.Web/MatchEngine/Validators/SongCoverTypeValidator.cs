namespace Audiotica.Web.MatchEngine.Validators
{
    public class SongCoverTypeValidator : BasicSongTypeValidatorBase
    {
        public override bool IsValid(string name, string matchName)
        {
            return IsTypeValid(name, matchName, "cover");
        }
    }
}