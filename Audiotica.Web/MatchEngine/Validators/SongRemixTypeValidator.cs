namespace Audiotica.Web.MatchEngine.Validators
{
    public class SongRemixTypeValidator : BasicSongTypeValidatorBase
    {
        public override bool IsValid(string name, string matchName)
        {
            return IsTypeValid(name, matchName, "remix", "mix", "rmx");
        }
    }
}