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
        Dictionary<string, int> map;
        List<string> categoryEntries;
        List<string> productEntries;
        StringBuilder sb;

        public QueryFile(string filePath)
        {
            this.filePath = filePath;
            map = new Dictionary<string, int>();
            categoryEntries = new List<string>();

            productEntries = new List<string>();
            sb = new StringBuilder();
        }

        public void WriteCategory(Category category)
        {
            string entry = $"('{category.Name}')";
            map[category.Name] = categoryEntries.Count + 1;
            categoryEntries.Add(entry);
        }

        /// <summary>
        /// Writes product record into query file container.
        /// </summary>
        /// <param name="product"></param>
        public void WriteProduct(Product product)
        {
            int categoryId = map[product.Category.Name];
            string entry = $"('{product.Name}', '{product.Description}', '{product.LogoImage}', '{product.PosterImage}', '{product.TechInfo}', '{product.Stock}', '{categoryId}', '{product.Price}')";
            productEntries.Add(entry);
        }

        /// <summary>
        /// Write query file container on the disk and closes the file stream.
        /// </summary>
        public void Close()
        {
            if(categoryEntries.Count > 0)
            {
                sb.Append("insert into Category(Name) values");
                int n = categoryEntries.Count;

                sb.Append($"{categoryEntries[0]},");
                sb.AppendLine();

                if (categoryEntries.Count < 2)
                {
                    sb.Append(";");
                    sb.AppendLine();
                }
                else
                {
                    for (int k = 1; k < n - 1; k++)
                    {
                        sb.Append($"{categoryEntries[k]},");
                        sb.AppendLine();
                    }

                    sb.Append($"{categoryEntries[n - 1]};");
                }
            }

            sb.AppendLine();

            if(productEntries.Count > 0)
            {
                sb.Append("insert into Product(Name, Description, LogoImage, PosterImage, TechInfo, Stock, CategoryId, Price) values");
                int n = productEntries.Count;

                sb.Append($"{productEntries[0]},");
                sb.AppendLine();

                if (productEntries.Count < 2)
                {
                    sb.Append(";");
                    sb.AppendLine();
                }
                else
                {
                    for (int k = 1; k < n - 1; k++)
                    {
                        sb.Append($"{productEntries[k]},");
                        sb.AppendLine();
                    }

                    sb.Append($"{productEntries[n - 1]};");
                }
            }

            if(!string.IsNullOrEmpty(sb.ToString()))
            {
                File.WriteAllText(filePath, sb.ToString());
                productEntries.Clear();
                categoryEntries.Clear();

                map.Clear();
                sb.Clear();
            }
        }
    }
}
