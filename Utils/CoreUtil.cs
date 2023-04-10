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
using BotanicTool.Data;
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
        public static async Task GetProductsAsync(string path)
        {
            var doc = new HtmlDocument();
            var client = new HttpClient();

            var baseUrl = ConfigurationManager.AppSettings["BASE_URL"];
            HttpResponseMessage res = await client.GetAsync(baseUrl);
            List<Product> products = new List<Product>();

            string str = await res.Content.ReadAsStringAsync();
            doc.LoadHtml(str);

            // find category list node
            HtmlNode productsNode = doc.DocumentNode.Descendants()
                .Where(n => n.HasAttributes && n.Attributes.Contains("id") && n.Attributes["id"].Value == "nav")
                .FirstOrDefault();

            // get categories for each link
            var categoryList = productsNode.Descendants()
                .Where(n => n.HasClass("level-top"))
                .ToList();

            string folder = ConfigurationManager.AppSettings["PRODUCT_SMALL_FOLDER"];
            string destFolder = Path.Combine(path, folder);

            foreach (var category in categoryList)
            {
                Category model = new Category();
                model.Name = category.InnerText;
                string link = category.ChildNodes[0].Attributes["href"].Value;

                try
                {
                    res = await client.GetAsync($"{baseUrl}{link}");
                }
                catch
                {
                    continue;
                }

                str = await res.Content.ReadAsStringAsync();
                doc.LoadHtml(str);

                var productNodes = doc.DocumentNode.Descendants()
                    .Where(n => n.HasClass("product_grid_cover"))
                    .ToList();

                for(int j = 0; j < productNodes.Count; j++)
                {
                    var imageLink = productNodes[j].ChildNodes[1]
                        .ChildNodes[1].ChildNodes[0].Attributes["src"].Value;

                    var productNameDiv = productNodes[j].Descendants()
                        .FirstOrDefault(c => c.HasClass("product-name"));

                    string url = productNameDiv.ChildNodes[0].Attributes["href"].Value;
                    string productName = productNameDiv.ChildNodes[0].Attributes["title"].Value;


                    var priceNode = productNodes[j].Descendants()
                        .FirstOrDefault(n => n.HasClass("price"));

                    string priceValue = priceNode != null ? priceNode.InnerText : null;
                    string priceFormatted = string.Empty;

                    if (priceValue != null)
                    {
                        int index = priceValue.IndexOf("L");
                        priceFormatted = priceValue.Substring(0, index - 1)
                            .Replace(".", "").Replace(',', '.');
                    }

                    double? price = priceNode != null ? double.Parse(priceFormatted) : null;
                    string imagePath = await DownloadImage(imageLink, destFolder, IsProduct: 1);
                    bool IsAvailable = price != null;

                    Product product = new Product
                    {
                        Link = url,
                        Name = productName,
                        LogoImage = imagePath,
                        IsAvailable = IsAvailable,
                        Category = model,
                        Price = price
                    };

                    products.Add(product);
                }
            }

            string destPath = Path.Combine(path, "products.sql");
            var queryFile = new QueryFile(destPath);

            folder = ConfigurationManager.AppSettings["PRODUCT_HUGE_FOLDER"];
            destFolder = Path.Combine(path, folder);

            for (int k = 0; k < products.Count; k++)
            {
                res = await client.GetAsync($"{products[k].Link}");
                str = await res.Content.ReadAsStringAsync();
                doc.LoadHtml(str);


                var posterImageDiv = doc.DocumentNode.Descendants()
                    .FirstOrDefault(n => n.HasClass("product-image-zoom"));

                if (posterImageDiv != null)
                {
                    string posterLink = posterImageDiv.ChildNodes[1].ChildNodes[1].Attributes["src"].Value;
                    string posterImagePath = await DownloadImage(posterLink, destFolder, IsProduct: 2);
                    products[k].PosterImage = posterImagePath;
                }
                else
                    products[k].PosterImage = $"./{folder.Replace("\\", "/")}/defaultPoster.png";

                var aboutNode = doc.DocumentNode.Descendants()
                    .FirstOrDefault(n => n.HasClass("box-description"));
                
                var techNode = doc.DocumentNode.Descendants()
                    .FirstOrDefault(n => n.HasClass("box-additional"));

                if (aboutNode != null) products[k].Description = aboutNode.InnerHtml;
                if(techNode != null) products[k].TechInfo = techNode.InnerHtml;

                queryFile.WriteRecord(products[k]);
            }

            queryFile.Close();
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
        static async Task<string> DownloadImage(string url, string destFolder, int IsProduct = 0)
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
            string key = IsProduct == 0 ? "PLANT_FOLDER" : (IsProduct == 1 ? "PRODUCT_SMALL_FOLDER" : "PRODUCT_HUGE_FOLDER");
            string relFolder = ConfigurationManager.AppSettings[key];

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
