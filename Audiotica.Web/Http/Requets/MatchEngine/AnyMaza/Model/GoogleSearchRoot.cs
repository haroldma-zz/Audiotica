using System.Collections.Generic;

namespace Audiotica.Web.Http.Requets.MatchEngine.AnyMaza.Model
{
    public class GoogleResult
    {
        public string GsearchResultClass { get; set; }
        public string UnescapedUrl { get; set; }
        public string Url { get; set; }
        public string VisibleUrl { get; set; }
        public string CacheUrl { get; set; }
        public string Title { get; set; }
        public string TitleNoFormatting { get; set; }
        public string Content { get; set; }
    }

    public class GooglePage
    {
        public string Start { get; set; }
        public int Label { get; set; }
    }

    public class GoogleCursor
    {
        public string ResultCount { get; set; }
        public List<GooglePage> Pages { get; set; }
        public string EstimatedResultCount { get; set; }
        public int CurrentPageIndex { get; set; }
        public string MoreResultsUrl { get; set; }
        public string SearchResultTime { get; set; }
    }

    public class GoogleSearchResponseData
    {
        public List<GoogleResult> Results { get; set; }
        public GoogleCursor Cursor { get; set; }
    }

    public class GoogleSearchRoot
    {
        public GoogleSearchResponseData ResponseData { get; set; }
        public object ResponseDetails { get; set; }
        public int ResponseStatus { get; set; }
    }
}