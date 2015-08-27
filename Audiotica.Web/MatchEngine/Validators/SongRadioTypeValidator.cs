namespace Audiotica.Web.MatchEngine.Validators
{
    public class SongRadioTypeValidator : BasicSongTypeValidatorBase
    {
        public override bool IsValid(string name, string matchName)
        {
            return IsTypeValid(name, matchName, "radio");
        }
    }
}