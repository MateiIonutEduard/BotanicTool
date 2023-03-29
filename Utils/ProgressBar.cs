using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotanicTool.Utils
{
    /// <summary>
    /// Provides the progress bar feature.
    /// </summary>
    public class ProgressBar
    {
        const char blockModel = '■';
        const string backLiteral = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";

        /// <summary>
        /// Show current percentage to progress bar.
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="update"></param>
        public static void WriteProgress(int percent, bool update = false)
        {
            if (update) Console.Write(backLiteral);
            Console.Write("[");

            // scale the percentage at lower range
            int percentage = (int)((percent / 100f) * 20f);
            Console.ForegroundColor = ConsoleColor.Cyan;

            for (int index = 0; index < 20; index++)
            {
                if (index >= percentage)
                    Console.Write(' ');
                else
                    Console.Write(blockModel);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] {0,3:##0}%", percent);
        }
    }
}
