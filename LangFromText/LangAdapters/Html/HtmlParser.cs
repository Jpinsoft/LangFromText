using HtmlAgilityPack;
using Jpinsoft.LangTainer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Jpinsoft.LangTainer.LangAdapters.Html
{
    public class HtmlParser
    {
        #region Props
        private char[] endChars = { '.', '?', '!' };
        private Random rnd = new Random();

        private Uri siteUri = null;

        public Uri SiteUri
        {
            get { return siteUri; }
            set { siteUri = value; }
        }

        #endregion

        private HtmlScanResult urlScanResult = new HtmlScanResult();
        public int RequestTimeOut { get; private set; }

        public HtmlParser()
        {
        }

        public HtmlParser(Uri siteUri, int requestTimeOut)
        {
            this.siteUri = siteUri;
            urlScanResult.URL = siteUri;
            RequestTimeOut = requestTimeOut;
        }

        /// <summary>
        /// Nasetuje na vsetkych potomkoch siteText
        /// </summary>
        /// <param name="rootScanResult"></param>
        public void SetSiteText(HtmlScanResult rootScanResult)
        {
            if (rootScanResult.RawData != null && rootScanResult.RawData.Length > 0)
            {
                HtmlDocument htmlDocument = new HtmlDocument();

                using (MemoryStream ms = new MemoryStream(rootScanResult.RawData))
                {
                    if (rootScanResult.RawDataEncoding == null)
                        htmlDocument.Load(ms);
                    else
                        htmlDocument.Load(ms, Encoding.GetEncoding(rootScanResult.RawDataEncoding));
                }

                StringBuilder sb = new StringBuilder();
                GetTextFromNodes(htmlDocument.DocumentNode, sb);
                rootScanResult.SiteText = WebUtility.HtmlDecode(sb.ToString());
            }

            foreach (var childUrlScanResult in rootScanResult.ChildWebSearchResults)
                SetSiteText(childUrlScanResult);
        }

        /// <summary>
        /// Pokusi sa parsovat url, ktoreho mime type je text.
        /// Ak sa nejedna o text content je vratene null
        /// </summary>
        /// <returns></returns>
        public HtmlScanResult ParseURL()
        {
            HtmlWeb agilityHtmlWeb = new HtmlWeb();
            HtmlDocument htmlDocument = agilityHtmlWeb.Load(siteUri);

            HtmlNode bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");

            if (bodyNode == null)
            {
                Console.WriteLine($"--- Skipping '{siteUri}' - body element is missing.");
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                // TODO skusit iba TEXT elementy
                GetTextFromNodes(bodyNode, sb);
                urlScanResult.SiteText = WebUtility.HtmlDecode(sb.ToString());
            }

            return urlScanResult;
        }

        private void GetTextFromNodes(HtmlNode currentNode, StringBuilder sb)
        {
            MineURL(currentNode);
            // ?? Musi byt ToLower, zvazit iba obsah P elementov
            if (currentNode is HtmlCommentNode || currentNode.Name.ToLower() == "script" || currentNode.Name.ToLower() == "code")
                return;

            // Paragraph sa rekurzivne nerozklada
            if (currentNode.Name.ToLower() == "p")
            {
                string trimmedText = currentNode.InnerText.Trim();

                if (!string.IsNullOrEmpty(trimmedText))
                {
                    // Vyhadzovat InnerText obsahujuci napr. < [
                    sb.Append(trimmedText);

                    if (!endChars.Contains(trimmedText.Last()))
                        sb.Append(".");

                    sb.AppendLine();
                    sb.AppendLine();
                }

                return;
            }

            foreach (HtmlNode childNode in currentNode.ChildNodes)
            {
                this.GetTextFromNodes(childNode, sb);
            }

            //if (currentNode is HtmlTextNode)
            //{
            //    string trimmedText = currentNode.InnerText.Trim();

            //    if (!string.IsNullOrEmpty(trimmedText))
            //    {
            //        // Vyhadzovat InnerText obsahujuci napr. < [
            //        sb.Append(trimmedText);

            //        //if (!trimmedText.EndsWith("."))
            //        //    sb.Append(".");

            //        sb.AppendLine();
            //        sb.AppendLine();
            //    }
            //}

            //// Neviem ci je to takto lepsie
            //if (currentNode.Name == "a" || currentNode.Name == "span")
            //    sb.Append(" ");
        }

        private void MineURL(HtmlNode node)
        {
            // OVERENE: name je vzdy malymi pismenami

            if (node.Name == "a")
            {
                HtmlAttribute hrefAtt = node.Attributes["href"];

                if (hrefAtt == null || string.IsNullOrEmpty(hrefAtt.Value))
                    return;

                string url = hrefAtt.Value.ToLower();

                if (url.StartsWith(@"javascript:") || url.Contains("#"))
                {
                    urlScanResult.IgnoredHrefs.Add(url);
                    return;
                }

                Uri result;
                Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out result);

                if (result == null)
                {
                    urlScanResult.IgnoredHrefs.Add(url);

                    return;
                }

                if (!result.IsAbsoluteUri)
                {
                    string rightSide = result.OriginalString;

                    if (rightSide.StartsWith("/"))
                        rightSide = rightSide.Remove(0, 1);

                    Uri.TryCreate(siteUri, "../" + rightSide, out result);

                    if (result == null)
                    {
                        urlScanResult.IgnoredHrefs.Add(url);
                        return;
                    }
                }

                // Pretoze niektore URI su sice created ale pri serializacii padnu
                try { var temp = result.ToString(); }
                catch { return; }

                urlScanResult.ContentHrefs.Add(result);
                RandomTools.Shuffle(urlScanResult.ContentHrefs, rnd);
            }
        }
    }
}
