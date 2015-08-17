using Audiotica.Core.Utilities.Interfaces;
using Audiotica.Web.Enums;
using Audiotica.Web.Metadata.Interfaces;

namespace Audiotica.Web.Metadata.Providers
{
    public abstract class MetadataProviderBase : IMetadataProvider
    {
        private readonly ISettingsUtility _settingsUtility;

        protected MetadataProviderBase(ISettingsUtility settingsUtility)
        {
            _settingsUtility = settingsUtility;
        }

        public abstract ProviderSpeed Speed { get; }
        public abstract ProviderCollectionSize CollectionSize { get; }
        public abstract ProviderCollectionType CollectionType { get; }
        public abstract string DisplayName { get; }
        public int Priority => (int) CollectionSize + (int) Speed + (int) CollectionType;

        public bool IsEnabled
        {
            get { return _settingsUtility.Read($"metadata_provider_enabled_{DisplayName}", true); }

            set { _settingsUtility.Write($"metadata_provider_enabled_{DisplayName}", value); }
        }
    }
}