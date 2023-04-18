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
            string? help = args.FirstOrDefault(arg => arg == "-h" || arg == "--help");
            string? path = args.FirstOrDefault(arg => arg == "-o" || arg == "--output");

            string? folderPath = args.SkipWhile(arg => arg == "-o" || arg == "--output").FirstOrDefault();

            if (!string.IsNullOrEmpty(help))
            {
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Usage: BotanicTool [-o] <directory_path>");
                    Console.WriteLine("\nOptions:");
                    Console.WriteLine("  -o, --output  specify directory path");
                    Console.WriteLine("  -h, --help  show this help message and exit");
                    Environment.Exit(-1);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Specify only destination folder path.");
                    Environment.Exit(-1);
                }
            }

            if(string.IsNullOrEmpty(folderPath))
            {
                Console.WriteLine("Specify only destination folder path.");
                Environment.Exit(-1);
            }
            
            // save plants list at specified file
            string plantsPath = Path.Combine(folderPath, "plants.json");
            string data = await CoreUtil.GetPlantItems(plantsPath);
            await File.WriteAllTextAsync(plantsPath, data);

            // save sql file that contains products and categories
            await CoreUtil.GetProductsAsync(folderPath);
        }
    }
}