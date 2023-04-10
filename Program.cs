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

            // save plants list at specified file
            //string data = await CoreUtil.GetPlantItems("plants.json");
            //await File.WriteAllTextAsync(plantsPath, data);

            // save sql file that contains products and categories
            await CoreUtil.GetProductsAsync("C:\\Users\\eduar\\Desktop\\test");
        }
    }
}