using BotanicTool.Models;
using Newtonsoft.Json;
using System.IO;

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

            string data = await CoreUtil.GetItems(args[0]);
            await File.WriteAllTextAsync(args[0], data);
        }
    }
}