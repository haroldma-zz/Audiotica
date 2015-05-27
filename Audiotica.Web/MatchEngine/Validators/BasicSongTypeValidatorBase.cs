using System.Linq;
using Audiotica.Web.MatchEngine.Interfaces.Validators;

namespace Audiotica.Web.MatchEngine.Validators
{
    /// <summary>
    ///     A base for implementing <see cref="ISongTypeValidator" /> that comes with a
    ///     <see cref="IsTypeValid" />
    ///     helper that uses the <see cref="SongTypeValidatorBase.IsNameValidType" /> helper to determine if is valid.
    /// </summary>
    public abstract class BasicSongTypeValidatorBase : SongTypeValidatorBase
    {
        /// <summary>
        ///     Validates the specified name against each type provided.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="matchName">Title of the match.</param>
        /// <param name="types">The types, each one should be interchangeable.</param>
        /// <returns></returns>
        protected bool IsTypeValid(string name, string matchName, params string[] types)
        {
            name = name.ToLower();
            matchName = matchName.ToLower();
            var isSupposedType = false;
            var isType = false;

            foreach (var loweredType in types.Select(type => type.ToLower()))
            {
                if (!isType)
                    isType = IsNameValidType(matchName, loweredType);

                if (!isSupposedType)
                    isSupposedType = IsNameValidType(name, loweredType);

                if (isSupposedType && isType)
                    return true;
            }

            return !isSupposedType && !isType;
        }
    }
}