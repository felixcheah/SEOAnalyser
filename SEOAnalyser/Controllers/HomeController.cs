using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEOAnalyser.Models;

namespace SEOAnalyser.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "SEO Analyser User Guide";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult SeoAnalyser()
        {
            return View();
        }

        public JsonResult GetRecords()
        {
            return Json(new { data = new SeoResult() });
        }

        [HttpPost]
        public JsonResult GetUrlSource(string url, List<string> keywordList)
        {
            url = url.Substring(0, 4) != "http" ? "http://" + url : url;
            using (var client = new WebClient())
            {
                try
                {
                    var htmlCode = client.DownloadString(url);
                    var result = WordCount(htmlCode, keywordList);
                    var jsonData = new { data = result };
                    return Json(jsonData);
                }
                catch (Exception ex)
                {
                    // ignored
                    return Json(new SeoResult());
                }
            }
        }

        private List<SeoResult> WordCount(string html, List<string> keywordList)
        {
            var seoResult = new List<SeoResult>();
            var source = html.Split(new[] { '.', '?', '!', ' ', ';', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var keyword in keywordList)
            {
                // Create the query.  Use ToLowerInvariant to match "data" and "Data"   
                var matchQuery = from word in source
                    where word.ToLowerInvariant() == keyword.ToLowerInvariant()
                    select word;

                // Count the matches, which executes the query.  
                var wordCount = matchQuery.Count();
                seoResult.Add(new SeoResult
                                {
                                    Keyword = keyword,
                                    Occurance = wordCount
                                });
            }

            //Get external links
            var externalLinks = Regex.Matches(html, @"<(a|link).*?href=(""|')(.+?)(""|').*?>");
            seoResult.Add(new SeoResult
                            {
                                Keyword = "External Link",
                                Occurance = externalLinks.Count
                            });

            var metaTag = new Regex(@"<meta\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'" +
                                      @"[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>",
                                    RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            var metaInformation = new Dictionary<string, string>();

            foreach (Match m in metaTag.Matches(html))
            {
                var metaContentTag = new Regex(@"(?<name>\b(\w|-)+\b)\" +
                                               @"s*=\s*(""(?<value>" +
                                               @"[^""]*)""|'(?<value>[^']*)'" +
                                               @"|(?<value>[^""'<> ]+)\s*)+",
                                                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                var matchs = metaContentTag.Match(m.Value.ToString());
                //metaInformation.Add(m.Groups[1].Value, m.Groups[2].Value);
            }

            return seoResult;
        }
    }
}
