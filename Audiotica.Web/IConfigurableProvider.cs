namespace Audiotica.Web
{
    public interface IConfigurableProvider
    {
        string DisplayName { get; }
        int Priority { get; }
        bool IsEnabled { get; set; }
    }
}