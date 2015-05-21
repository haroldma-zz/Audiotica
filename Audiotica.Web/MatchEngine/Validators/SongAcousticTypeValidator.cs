namespace Audiotica.Web.MatchEngine.Validators
{
    public class SongAcousticTypeValidator : BasicSongTypeValidatorBase
    {
        public override bool IsValid(string name, string matchName)
        {
            return IsTypeValid(name, matchName, "acoustic");
        }
    }
}