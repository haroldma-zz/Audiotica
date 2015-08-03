namespace Audiotica.Web.MatchEngine.Validators
{
    public class SongSlowedTypeValidator : BasicSongTypeValidatorBase
    {
        public override bool IsValid(string name, string matchName)
        {
            return IsTypeValid(name, matchName, "slowed", "slow down", "slow mo");
        }
    }
}