using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using YelpAPI;

namespace FoodaCore
{
    public class FoodaMenu
    {
        public FoodaMenu(string html, string url)
        {
            const string restaurantName = "restaurant-banner__name";
            const string restaurantDesc = "restaurant-banner__description";
            const string categoryGroup = "item-group--list";
            const string categoryClass = "item-group__category";
            const string itemClass = "item__link js-item-show-link";
            const string itemName = "item__name";
            const string itemPrice = "item__price";
            const string itemDesc = "item__desc__text";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            Url = url;
            Name = SafeGetHeaderValue(htmlDoc, restaurantName, "(no name provided)");
            Rating = GetYelpRating(Name);
            Description = SafeGetHeaderValue(htmlDoc, restaurantDesc, "(no description provided)");            
            Categories = new List<FoodaCategory>();

            foreach (var htmlCategory in htmlDoc.DocumentNode.Descendants("div").Where(d => d.Attributes["class"].Value == categoryGroup))
            {
                var newCategory = new FoodaCategory
                {
                    CategoryName = CleanUpText(htmlCategory.Descendants("h2").First(d => d.Attributes["class"].Value == categoryClass).InnerText),
                    Items = new List<FoodaItem>()
                };

                foreach (var htmlItem in htmlCategory.Descendants("a").Where(d => d.Attributes["class"].Value == itemClass))
                {
                    var newItem = new FoodaItem
                    {
                        Name = CleanUpText(htmlItem.Descendants("div")
                            .First(d => d.Attributes["class"].Value == itemName)
                            .InnerText),
                        Price = CleanUpText(htmlItem.Descendants("div")
                            .First(d => d.Attributes["class"].Value == itemPrice)
                            .InnerText),
                        Description = CleanUpText(htmlItem.Descendants("div")
                            .First(d => d.Attributes["class"].Value == itemDesc)
                            .InnerText)
                    };

                    newCategory.Items.Add(newItem);

                }
                Categories.Add(newCategory);
            }

        }

        private static string SafeGetHeaderValue(HtmlDocument htmlDoc, string restaurantname, string defaultText)
        {
            try
            {
                return CleanUpText(htmlDoc.DocumentNode.Descendants("div").First(d => d.Attributes["class"].Value == restaurantname).InnerText);
            }
            catch (Exception)
            {
                return defaultText;
            }            
        }

        private decimal? GetYelpRating(string name)
        {
            try
            {
                var client = new YelpAPIClient();
                JObject response = client.Search(name, "Chicago, IL");
                JArray businesses = (JArray)response.GetValue("businesses");

                if (businesses.Count == 0)
                    return null;

                string business_id = (string)businesses[0]["id"];
                string yelpName = (string)businesses[0]["name"];

                if (yelpName.Trim() != name.Trim())
                    return null;

                response = client.GetBusiness(business_id);
                string temp = (string)response["rating"];
                return decimal.Parse(temp);
            }
            catch
            {
                return null;
            }
        }

        public static string CleanUpText(string text)
        {
            return HttpUtility.HtmlDecode(text).Replace("\r", "").Replace("\n", "").Trim();
        }

        public string Name { get; set; }
        public decimal? Rating { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }

        public List<FoodaCategory> Categories { get; set; }

        public string SlackMessage()
        {
            var str = string.Empty;

            str += $"*{Name}*\n";
            str += $"{WordWrapAndIndent(Description, 120, 0)}\n";
            str += $"<{Url}|View Full Menu> \n{GetRatingText(Rating)}\n\n";

            foreach (var c in Categories)
            {
                str += $"*{c.CategoryName}*\n```";
                foreach (var i in c.Items)
                {
                    str += $"{i.Name,-95}{i.Price}\n";
                    str += $"{WordWrapAndIndent(i.Description, 70, 2)}\n";
                }
                str += "```\n\n\n";
            }

            return str;
        }

        private string GetRatingText(decimal? rating)
        {
            if (!rating.HasValue)
                return string.Empty;

            string temp = string.Empty;
            decimal i;

            for (i = 1; i <= 5; i++)
            {
                if(Math.Floor(rating.Value) >= i)
                    temp += ":fullstarrating:";
                else if(rating.Value + 0.5m == i)
                    temp += ":halfstarrating:";
                else
                    temp += ":nostarrating:";

            }            

            return temp + "\n";
        }

        public static string WordWrapAndIndent(string text, int width, int indent)
        {
            int pos, next;
            StringBuilder sb = new StringBuilder();

            var indentStr = new string(' ', indent);
            sb.Append(indentStr);

            // Lucidity check
            if (width < 1)
                return text;

            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                int eol = text.IndexOf(Environment.NewLine, pos);
                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + Environment.NewLine.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;
                        if (len > width)
                            len = BreakLine(text, pos, width);
                        sb.Append(text, pos, len);
                        sb.Append(Environment.NewLine + indentStr);

                        // Trim whitespace following break
                        pos += len;
                        while (pos < eol && Char.IsWhiteSpace(text[pos]))
                            pos++;
                    } while (eol > pos);
                }
                else sb.Append(Environment.NewLine); // Empty line
            }
            return sb.ToString();
        }

        /// <summary>
        /// Locates position to break the given line so as to avoid
        /// breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        private static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max;
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;

            // If no whitespace found, break at maximum length
            if (i < 0)
                return max;

            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;

            // Return length of text before whitespace
            return i + 1;
        }
    }
}