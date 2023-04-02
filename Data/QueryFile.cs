using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotanicTool.Data
{
    /// <summary>
    /// Represents SQL query handler that it is saved to output file on the disk.
    /// </summary>
    public class QueryFile
    {
        string filePath;
        List<string> entries;
        StringBuilder sb;

        public QueryFile(string filePath)
        {
            this.filePath = filePath;
            entries = new List<string>();
            sb = new StringBuilder();
        }

        /// <summary>
        /// Writes product record into query file container.
        /// </summary>
        /// <param name="product"></param>
        public void WriteRecord(Product product)
        {
            string entry = $"('{product.Name}', '{product.Description}', '{product.TechInfo}', '{product.Price}')";
            entries.Add(entry);
        }

        /// <summary>
        /// Write query file container on the disk and closes the file stream.
        /// </summary>
        public void Close()
        {
            sb.Append("insert into Product(Name, Description, TechInfo, Price) values");
            
            if(entries.Count > 0)
            {
                int n = entries.Count;
                sb.Append($"{entries[0]},");
                sb.AppendLine();

                if (entries.Count < 2)
                {
                    sb.Append(";");
                    sb.AppendLine();
                }
                else
                {
                    for (int k = 1; k < n - 1; k++)
                    {
                        sb.Append($"{entries[k]},");
                        sb.AppendLine();
                    }

                    sb.Append($"{entries[n - 1]};");
                }

                File.WriteAllText(filePath, sb.ToString());
                entries.Clear();
                sb.Clear();
            }
        }
    }
}
