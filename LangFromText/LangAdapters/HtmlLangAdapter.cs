using HtmlAgilityPack;
using Jpinsoft.LangTainer.CBO;
using Jpinsoft.LangTainer.LangAdapters.Html;
using Jpinsoft.LangTainer.Types;
using Jpinsoft.LangTainer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jpinsoft.LangTainer.LangAdapters
{
    public class HtmlLangAdapter : ILangAdapter
    {
        public object AdapterSettings { get; set; } = new HtmlLangAdapterSettings { HtmlScanLevel = 2 };
        public HtmlLangAdapterSettings HtmlAdapterSettings { get { return (HtmlLangAdapterSettings)AdapterSettings; } }

        private List<Uri> AnalyzedSites { get; set; } = new List<Uri>();
        private const bool CN_SKIP_VISITED_URL = true;
        private int nextRequestPause = 500;
        private bool cancelSignal = false;
        public int RequestTimeOut { get; private set; } = 10_000;

        public HtmlLangAdapter()
        {
            cancelSignal = false;
        }

        public void CancelOperation()
        {
            cancelSignal = true;
        }

        public List<TextSourceCBO> GetTextSources(string sourceAddress, Action<object> progressChanged = null)
        {
            AnalyzedSites.Clear();
            List<TextSourceCBO> flatListRes = new List<TextSourceCBO>();

            // ServicePointManager.SecurityProtocol =  SecurityProtocolType.Tls12;

            HtmlScanResult htmlScanResult = new HtmlScanResult(new List<Uri>() { new Uri(sourceAddress.ToLower()) });

            //Task.Factory.StartNew(new Action(delegate ()
            //{})).Wait();
            this.ScanURISynchronous(htmlScanResult, progressChanged);

            // TreeToFlatResults(res, htmlScanResult, false);
            TreeToFlatDistinctHostResults(flatListRes, htmlScanResult);
            SHA1 sha1 = SHA1.Create();
            flatListRes.ForEach(item => item.Sha1Hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(item.Text.ToString())));

            return flatListRes;
        }

        private List<TextSourceCBO> TreeToFlatDistinctHostResults(List<TextSourceCBO> res, HtmlScanResult htmlScanResult)
        {
            if (htmlScanResult.Level > 0)
            {
                if (!htmlScanResult.IsError)
                {
                    // string address = $"{htmlScanResult.URL.Scheme}://{htmlScanResult.URL.Host}";
                    string address = htmlScanResult.URL.ToString().ToLower();

                    TextSourceCBO textSourceCBO = res.FirstOrDefault(tSource => tSource.Address == address);

                    if (textSourceCBO == null)
                    {
                        textSourceCBO = new TextSourceCBO
                        {
                            Address = address,
                            Created = DateTime.Now,
                            Text = new StringBuilder(),
                            TextSourceType = Enums.TextSourceTypeEnum.URL_HTML
                        };

                        res.Add(textSourceCBO);
                    }

                    textSourceCBO.Text.AppendLine(htmlScanResult.SiteText);
                }
            }

            foreach (HtmlScanResult innerHtmlScanResult in htmlScanResult.ChildWebSearchResults)
            {
                TreeToFlatDistinctHostResults(res, innerHtmlScanResult);
            }

            return res;
        }


        /// <summary>
        /// Nepouzite, nahradene s TreeToFlatDistinctHostResults
        /// </summary>
        private void TreeToFlatResults(List<TextSourceCBO> res, HtmlScanResult htmlScanResult, bool doHash)
        {
            if (htmlScanResult.Level > 0)
            {
                if (!string.IsNullOrEmpty(htmlScanResult?.SiteText))
                {
                    res.Add(new TextSourceCBO
                    {
                        Address = htmlScanResult.URL.ToString(),
                        Created = DateTime.Now,
                        Sha1Hash = doHash ? SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(htmlScanResult.SiteText)) : null,
                        Text = new StringBuilder(htmlScanResult.SiteText),
                        TextSourceType = Enums.TextSourceTypeEnum.URL_HTML
                    });
                }
            }

            foreach (HtmlScanResult innerHtmlScanResult in htmlScanResult.ChildWebSearchResults)
            {
                TreeToFlatResults(res, innerHtmlScanResult, doHash);
            }
        }


        /// <summary>
        /// Blocking recursive method
        /// </summary>
        private void ScanURISynchronous(HtmlScanResult parrentHtmlScanResult, Action<object> progressChanged = null)
        {
            if (parrentHtmlScanResult.Level >= HtmlAdapterSettings.HtmlScanLevel)
                return;

            foreach (Uri contentHref in parrentHtmlScanResult.ContentHrefs)
            {
                HtmlScanResult childResult;

                if (DoSkipURL(parrentHtmlScanResult, contentHref, progressChanged))
                    continue;

                try { childResult = new HtmlParser(contentHref, RequestTimeOut).ParseURL(); }
                catch (Exception ex) { childResult = new HtmlScanResult { URL = contentHref, ErrorMessage = ExceptionUtils.AddInnerMessages(ex) }; }

                // Pri synchronous musi byt az za ParseURL
                AnalyzedSites.Add(contentHref);

                if (childResult == null)
                    continue;

                childResult.Parrent = parrentHtmlScanResult;
                parrentHtmlScanResult.ChildWebSearchResults.Add(childResult);

                progressChanged?.Invoke(childResult);

                ScanURISynchronous(childResult, progressChanged);
            }
        }

        private bool DoSkipURL(HtmlScanResult parrentWebSearchResult, Uri uri, Action<object> progressChanged = null)
        {
            if (cancelSignal)
            {
                progressChanged?.Invoke(new HtmlScanResult { URL = uri, ErrorMessage = $"--- Terminating '{uri}' - operation was canceled." });
                return true;
            }

            try
            {
                if (parrentWebSearchResult.Level > 0 && !string.IsNullOrEmpty(HtmlAdapterSettings.AllowedURLContains) && !uri.OriginalString.ToLower().Contains(HtmlAdapterSettings.AllowedURLContains.ToLower()))
                {
                    progressChanged?.Invoke(new HtmlScanResult { URL = uri, ErrorMessage = $"--- Skipping '{uri}' - doesn't contains {HtmlAdapterSettings.AllowedURLContains}" });
                    return true;
                }

                if (parrentWebSearchResult.Level > 0 && !string.IsNullOrEmpty(HtmlAdapterSettings.AllowedURLNotContains) && uri.OriginalString.ToLower().Contains(HtmlAdapterSettings.AllowedURLNotContains.ToLower()))
                {
                    progressChanged?.Invoke(new HtmlScanResult { URL = uri, ErrorMessage = $"--- Skipping '{uri}' - contains {HtmlAdapterSettings.AllowedURLNotContains}" });
                    return true;
                }

                if (CN_SKIP_VISITED_URL && AnalyzedSites.Contains(uri))
                    return true;
            }
            catch { return true; }

            if (nextRequestPause > 0)
                Task.Delay(nextRequestPause).Wait();

            return false;
        }
    }
}
