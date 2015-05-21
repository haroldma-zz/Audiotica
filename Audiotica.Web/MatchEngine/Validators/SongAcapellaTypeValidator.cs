namespace Audiotica.Web.MatchEngine.Validators
{
    public class SongAcapellaTypeValidator : BasicSongTypeValidatorBase
    {
        public override bool IsValid(string name, string matchName)
        {
            return IsTypeValid(name, matchName, "acapella");
        }
    }
}