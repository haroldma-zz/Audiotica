using Audiotica.Web.MatchEngine.Interfaces.Validators;

namespace Audiotica.Web.MatchEngine.Validators
{
    /// <summary>
    ///     A base for implementing <see cref="ISongTypeValidator" /> that comes with a <see cref="IsNameValidType" /> helper.
    /// </summary>
    public abstract class SongTypeValidatorBase : ISongTypeValidator
    {
        public abstract bool IsValid(string name, string matchName);

        protected bool IsNameValidType(string name, string type)
        {
            return name.Contains($" {type} ") || name.EndsWith($" {type}") || name.StartsWith($"{type} ")
                   || name.Contains($"({type} ") || name.Contains($" {type})")
                   || name.Contains($"\"{type}\"")
                   || name.Contains($"'{type}'")
                   || name.Contains($"{type}*")
                   || name.Contains($"*{type}")
                   || name.Contains($" {type},")
                   || name.Contains($" {type};");
        }
    }
}