using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Configuration;
using BotanicTool.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Policy;
#pragma warning disable

namespace BotanicTool.Utils
{
    public class CoreUtil
    {
        /// <summary>
        /// Provides json content from plants description objects.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetPlantItems(string path)
        {
            var doc = new HtmlDocument();
            var client = new HttpClient();

            var hashSet = new HashSet<string>();
            var baseUrl = ConfigurationManager.AppSettings["BASE_URL"];
            var plantList = new List<Plant>();

            var res = await client.GetAsync(baseUrl);
            string str = await res.Content.ReadAsStringAsync();
            doc.LoadHtml(str);

            var list = doc.DocumentNode.Descendants().Where(n => n.HasClass("left_side_item"));
            var classList = new List<string>();

            // get dest folder path
            string root = Path.GetDirectoryName(path);
            string folder = ConfigurationManager.AppSettings["PLANT_FOLDER"];
            string destFolder = Path.Combine(root, folder);

            // get plant categories
            foreach (var node in list)
            {
                var href = node.ChildNodes[1];
                classList.Add(href.InnerHtml);
            }

            // fetch plants
            var plants = doc.DocumentNode.Descendants()
                .Where(n => n.HasClass("right_side_item"));

            string title = Console.Title;
            Console.Title = "In progress... 0%";

            Console.Write("Fetch Plants... ");
            ProgressBar.WriteProgress(0);
            int total = 0;

            // get plant description for each item from list
            foreach (var plant in plants)
            {
                var plantSpan = plant.Descendants().Where(n => n.HasClass("image")).FirstOrDefault();
                var plantImage = plantSpan.ChildNodes[0].Attributes["src"].Value;

                string link = $"{baseUrl}{plant.ChildNodes[1].Attributes["href"].Value}";
                int percent = (int)((double)total / plants.Count() * 100);

                Console.Title = $"In progress... {percent}%";
                ProgressBar.WriteProgress(percent, true);

                var plantName = plant.Descendants().Where(n => n.HasClass("category-name"))
                    .FirstOrDefault().InnerHtml;

                if (hashSet.Contains(plantName))
                {
                    total++;
                    continue;
                }

                string fullPath = await DownloadImage(plantImage, destFolder);
                var description = await GetDescription(link, destFolder);

                var item = new Plant
                {
                    name = plantName,
                    imageUrl = fullPath,
                    description = description,
                    category = plant.Attributes["data-category-name"].Value,
                };

                if (description != null)
                {
                    hashSet.Add(item.name);
                    plantList.Add(item);
                }
                else RemoveFile(plantImage, destFolder);
                total++;
            }

            Console.Title = title;
            ProgressBar.WriteProgress(100, true);
            Console.WriteLine("\nPlant list created successfully.");

            string data = JsonConvert.SerializeObject(plantList);
            return JsonPrettify(data);
        }

        /// <summary>
        /// Creates sql query file with categories records and product list.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<string> GetProductsAsync(string path)
        {
            var doc = new HtmlDocument();
            var client = new HttpClient();

            var baseUrl = ConfigurationManager.AppSettings["BASE_URL"];
            HashSet<string> hashSet = new HashSet<string>();

            HttpResponseMessage res = await client.GetAsync(baseUrl);
            string str = await res.Content.ReadAsStringAsync();
            doc.LoadHtml(str);

            return null;
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
        /// Download image to a specified directory.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="destFolder"></param>
        /// <returns></returns>
        static async Task<string> DownloadImage(string url, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            var tempUrl = url;
            var baseUrl = ConfigurationManager.AppSettings["BASE_URL"];
            if (!url.StartsWith(baseUrl)) tempUrl = $"{baseUrl}{url}";

            var client = new HttpClient();
            var res = await client.GetAsync(tempUrl);

            var stream = await res.Content.ReadAsStreamAsync();
            int index = tempUrl.LastIndexOf("/");

            string fileName = tempUrl.Substring(index + 1);
            string relFolder = ConfigurationManager.AppSettings["PLANT_FOLDER"];

            // save image to disk file
            string path = Path.Combine(destFolder, fileName);
            byte[] buffer = ((MemoryStream)stream).ToArray();

            File.WriteAllBytes(path, buffer);
            var newPath = Path.Combine(".", relFolder, fileName);

            newPath = newPath.Replace('\\', '/');
            return newPath;
        }

        static void RemoveFile(string url, string destFolder)
        {
            int index = url.LastIndexOf("/");
            string fileName = url.Substring(index + 1);

            string path = Path.Combine(destFolder, fileName);
            File.Delete(path);
        }

        /// <summary>
        /// Returns plant's description from specific url.
        /// </summary>
        /// <param name="url">Represents plant description link.</param>
        /// <param name="destFolder">Represents destination folder.</param>
        /// <returns></returns>
        static async Task<Description> GetDescription(string url, string destFolder)
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

                string relativePath = await DownloadImage(image, destFolder);

                if (taxonomyDescription == null)
                {
                    RemoveFile(image, destFolder);
                    throw new Exception("Product item is not valid.");
                }

                var description = new Description
                {
                    posterImage = relativePath,
                    body = taxonomyDescription.InnerHtml
                };

                return description;
            }
            catch (Exception)
            { return null; }
        }
    }
}
