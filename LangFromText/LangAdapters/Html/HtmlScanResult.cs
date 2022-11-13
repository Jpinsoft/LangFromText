using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jpinsoft.LangTainer.LangAdapters.Html
{
    public class HtmlScanResult
    {
        // public WebScanResult ParrentWebScanResult { get; set; }

        public HtmlScanResult()
        {
        }

        public HtmlScanResult(List<Uri> contentHrefs)
        {
            this.contentHrefs = contentHrefs;
            URL = new Uri(DateTime.Now.ToString("yyyyMMdd_HHmmss_scan"), UriKind.RelativeOrAbsolute);
        }

        public bool IsError { get { return !string.IsNullOrEmpty(ErrorMessage); } }

        public HtmlScanResult Parrent { get; set; }

        public int Level { get { return GetLevel(this, 0); } }

        public Uri URL { get; set; }
        public string SiteText { get; set; }

        public byte[] RawData { get; set; }

        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets the content type form the response.ContentType
        /// A string that contains the content type of the response.
        /// </summary>
        public string ContentType { get; set; }

        public string RawDataEncoding { get; set; }

        private List<Uri> contentHrefs = new List<Uri>();
        public List<Uri> ContentHrefs { get { return contentHrefs; } }

        private List<string> ignoredHrefs = new List<string>();
        public List<string> IgnoredHrefs { get { return ignoredHrefs; } }

        private List<HtmlScanResult> childWebSearchResults = new List<HtmlScanResult>();
        public List<HtmlScanResult> ChildWebSearchResults { get { return childWebSearchResults; } }

        #region Methods

        private int GetLevel(HtmlScanResult webSearchResult, int generation)
        {
            if (webSearchResult.Parrent == null)
                return generation;

            return GetLevel(webSearchResult.Parrent, generation + 1);
        }

        public int GetDescendantCount()
        {
            int res = this.ChildWebSearchResults.Count;

            foreach (var childResult in this.ChildWebSearchResults)
                res += childResult.GetDescendantCount();

            return res;
        }

        public int GetMaxLevel(int generation)
        {
            if (this.childWebSearchResults.Count == 0)
                return generation;

            List<int> res = new List<int>();

            this.ChildWebSearchResults.ForEach(scanRes => res.Add(scanRes.GetMaxLevel(generation + 1)));

            return res.Max();
        }

        public Dictionary<string, int> DistinctURL(bool fromScannedURL, bool disctincByDnsHost)
        {
            Dictionary<string, int> statistics = new Dictionary<string, int>();
            DistinctURL(statistics, fromScannedURL, disctincByDnsHost);
            return statistics;
        }

        private void DistinctURL(Dictionary<string, int> statistics, bool onlyScannedURL, bool disctinctByDnsHost)
        {
            string linkString;

            if (this.Level != 0)
            {
                if (onlyScannedURL)
                {
                    linkString = this.URL.OriginalString;

                    if (disctinctByDnsHost)
                        linkString = this.URL.DnsSafeHost;

                    if (!statistics.Keys.Contains(linkString))
                        statistics.Add(linkString, 1);
                    else
                        statistics[linkString] += 1;
                }
                else
                {
                    foreach (var link in this.ContentHrefs)
                    {
                        linkString = link.OriginalString;

                        if (disctinctByDnsHost)
                            linkString = link.DnsSafeHost;

                        if (!statistics.Keys.Contains(linkString))
                            statistics.Add(linkString, 1);
                        else
                            statistics[linkString] += 1;
                    }
                }
            }

            foreach (var childResult in this.ChildWebSearchResults)
                childResult.DistinctURL(statistics, onlyScannedURL, disctinctByDnsHost);
        }

        #endregion
    }
}
