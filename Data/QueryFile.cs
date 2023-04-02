using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotanicTool.Data
{
    public class QueryFile
    {
        string filePath;
        StringBuilder sb;

        public QueryFile(string filePath)
        {
            this.filePath = filePath;
            sb = new StringBuilder();
        }

        public void Close()
        {
            File.WriteAllText(filePath, sb.ToString());
            sb.Clear();
        }
    }
}
