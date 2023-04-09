using System.IO;
using Newtonsoft.Json;
using BotanicTool.Models;
using BotanicTool.Utils;

namespace BotanicTool
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            /*if(args.Length != 1)
            {
                Console.WriteLine("Specify only destination folder path.");
                Environment.Exit(-1);
            }*/

            // set plants file path and product sql file path
            string plantsPath = Path.Combine("Storage", "plants.json");
            string productsPath = Path.Combine("Storage", "products.sql");

            // save plants list at specified file
            //string data = await CoreUtil.GetPlantItems(plantsPath);
            //await File.WriteAllTextAsync(plantsPath, data);

            // save sql file that contains products and categories
            await CoreUtil.GetProductsAsync(productsPath);
        }
    }
}