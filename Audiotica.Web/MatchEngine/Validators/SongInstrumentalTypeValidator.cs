namespace Audiotica.Web.MatchEngine.Validators
{
    public class SongInstrumentalTypeValidator : BasicSongTypeValidatorBase
    {
        public override bool IsValid(string name, string matchName)
        {
            return IsTypeValid(name, matchName, "instrumental", "karaoke");
        }
    }
}