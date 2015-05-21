using System.Linq;

namespace Audiotica.Web.MatchEngine.Validators
{
    /// <summary>
    ///     Sometimes there are types that you might want to check agains't
    ///     but don't mind if the match doesn't has it.
    ///     <remarks>
    ///         Instead of creating different classes for flexible types, they should be added here.
    ///     </remarks>
    ///     <example>
    ///         "song (Spotify Edition)" would match "song" as the same type.
    ///         "song" would NOT match "song (Spotify Edition)" as the same type.
    ///     </example>
    /// </summary>
    public class FlexibleTypeValidator : SongTypeValidatorBase
    {
        private readonly string[] _flexibleTypes =
        {
            "edition"
        };

        public override bool IsValid(string name, string matchName)
        {
            return _flexibleTypes.All(type => IsTypeValid(name, matchName, type));
        }

        /// <summary>
        ///     Validates the specified name against the type provided.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="matchName">Name of the match.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected bool IsTypeValid(string name, string matchName, string type)
        {
            name = name.ToLower();
            matchName = matchName.ToLower();
            type = type.ToLower();

            var isType = IsNameValidType(matchName, type);
            var isSupposedType = IsNameValidType(name, type);

            return isSupposedType || !isType;
        }
    }
}