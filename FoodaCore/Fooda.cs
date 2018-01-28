using System;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace FoodaCore
{
    public class Fooda
    {
        public static string GetFoodaMenu(string foodaPopupId)
        {
            try
            {
                var calendarUri = "https://app.fooda.com/my?date=";
                var url = GetMenuUrl(DateTime.Now.Date, calendarUri, foodaPopupId);
                var html = LoadRawHtml(url);
                var fooda = new FoodaMenu(html, url);

                return fooda.SlackMessage();
            }
            catch (Exception e)
            {
                return $"Help me!  I think I'm broken: {e}";
            }

        }

        private static string GetMenuUrl(DateTime date, string calendarUri, string popupId)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var proxy = WebRequest.DefaultWebProxy;
            using (var client = new WebClient { Proxy = proxy, Encoding = Encoding.UTF8 })
            {

                client.Headers.Add(HttpRequestHeader.Cookie, $"context=%7B%22entity%22%3A%22popup_event%22%2C%22id%22%3A{popupId}%7D");

                var html = client.DownloadString(new Uri(calendarUri + date.ToString("yyyy-MM-dd")));
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                var url =
                    htmlDoc.DocumentNode.Descendants("a")
                        .First(d => d.Attributes["class"]?.Value == "myfooda-event__restaurant js-vendor-tile");

                return url.Attributes["href"].Value;
            }
        }


        private static string LoadRawHtml(string url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var proxy = WebRequest.DefaultWebProxy;
            using (var client = new WebClient { Proxy = proxy, Encoding = Encoding.UTF8 })
            {
                return client.DownloadString(new Uri(url));
            }
        }
    }
}
