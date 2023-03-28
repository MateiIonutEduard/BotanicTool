using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Configuration;
using BotanicTool.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
#pragma warning disable

namespace BotanicTool
{
    public class CoreUtil
    {
        /// <summary>
        /// Provides json content from plants description objects.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetItems()
        {
            var doc = new HtmlDocument();
            var client = new HttpClient();
            var baseUrl = ConfigurationManager.AppSettings["BASE_URL"];
            var plantList = new List<Plant>();

            var res = await client.GetAsync(baseUrl);
            string str = await res.Content.ReadAsStringAsync();
            doc.LoadHtml(str);

            var list = doc.DocumentNode.Descendants().Where(n => n.HasClass("left_side_item"));
            var classList = new List<string>();

            // get plant categories
            foreach(var node in list)
            {
                var href = node.ChildNodes[1];
                classList.Add(href.InnerHtml);
            }

            // fetch plants
            var plants = doc.DocumentNode.Descendants()
                .Where(n => n.HasClass("right_side_item"));

            string title = Console.Title;
            Console.Title = "In progress... 0%";
            int total = 0;

            // get plant description for each item from list
            foreach (var plant in plants)
            {
                var plantSpan = plant.Descendants().Where(n => n.HasClass("image")).FirstOrDefault();
                var plantImage = plantSpan.ChildNodes[0].Attributes["src"].Value;

                string link = $"{baseUrl}{plant.ChildNodes[1].Attributes["href"].Value}";
                var description = await GetDescription(link);

                var plantName = plant.Descendants().Where(n => n.HasClass("category-name"))
                    .FirstOrDefault().InnerHtml;

                var item = new Plant
                {
                    name = plantName,
                    imageUrl = plantImage,
                    description = description,
                    category = plant.Attributes["data-category-name"].Value,
                };

                Console.Title = $"In progress... {(int)(((double)total / plants.Count()) * 100)}%";
                if(description != null) plantList.Add(item);
                total++;
            }

            Console.Title = title;
            string data = JsonConvert.SerializeObject(plantList);
            return JsonPrettify(data);
        }

        /// <summary>
        /// Returns the prettified json content.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        static string JsonPrettify(string json)
        {
            using (var stringReader = new StringReader(json))
                using (var stringWriter = new StringWriter())
                {
                    var jsonReader = new JsonTextReader(stringReader);
                    var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                    jsonWriter.WriteToken(jsonReader);
                    return stringWriter.ToString();
                }
        }

    /// <summary>
    /// Returns plant's description from specific url.
    /// </summary>
    /// <param name="url">Represents plant description link.</param>
    /// <returns></returns>
    static async Task<Description> GetDescription(string url)
        {
            try
            {
                var doc = new HtmlDocument();
                var client = new HttpClient();

                var res = await client.GetAsync(url);
                string html = await res.Content.ReadAsStringAsync();
                doc.LoadHtml(html);

                var imageDiv = doc.DocumentNode.Descendants()
                    .Where(n => n.HasClass("community-category-header-info"))
                    .FirstOrDefault();

                var image = imageDiv.ChildNodes[1].ChildNodes[1].Attributes["src"].Value;
                var taxonomyDescription = doc.DocumentNode.Descendants()
                    .Where(n => n.HasClass("taxonomy-description")).FirstOrDefault();


                var description = new Description
                {
                    posterImage = image,
                    body = taxonomyDescription.InnerHtml
                };

                return description;
            }
            catch(Exception)
            { return null; }
        }
    }
}
