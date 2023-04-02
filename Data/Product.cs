﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable

namespace BotanicTool.Data
{
    /// <summary>
    /// Represents product object that references product sql table.
    /// </summary>
    public class Product
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TechInfo { get; set; }
        public double Price { get; set; }
    }
}