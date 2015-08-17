using System.Threading.Tasks;
using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Metadata.Interfaces;

namespace Audiotica.Web.Metadata.Providers
{
    /// <summary>
    ///     This base provider is use for lyrics, hence the CollectionSize and CollectionType are set to none.
    /// </summary>
    public abstract class MetadataProviderLyricsOnlyBase : MetadataProviderBase, ILyricsMetadataProvider
    {
        protected MetadataProviderLyricsOnlyBase(ISettingsUtility settingsUtility) : base(settingsUtility)
        {
        }

        public override ProviderCollectionSize CollectionSize => ProviderCollectionSize.None;
        public override ProviderCollectionType CollectionType => ProviderCollectionType.None;
        public abstract Task<string> GetLyricAsync(string song, string artist);
    }
}