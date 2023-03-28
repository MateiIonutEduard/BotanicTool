using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable

namespace BotanicTool.Models
{
    public class Plant
    {
        public string name { get; set; }
        public string imageUrl { get; set; }
        public Description description { get; set; }
        public string category { get; set; }
    }
}
