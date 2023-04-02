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
            if(args.Length != 1)
            {
                Console.WriteLine("Specify only file destination path.");
                Environment.Exit(-1);
            }

            string data = await CoreUtil.GetPlantItems(args[0]);
            await File.WriteAllTextAsync(args[0], data);
        }
    }
}