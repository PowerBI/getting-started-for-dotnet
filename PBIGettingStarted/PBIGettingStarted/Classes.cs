﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIGettingStarted
{
    public class jsonExample
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public bool IsCompete { get; set; }
        public DateTime ManufacturedOn { get; set; }
    }


    public class Music
    {
        public string Artist { get; set; }
        public string Song { get; set; }
        public string Genre { get; set; }
        public string Location { get; set; }
        public DateTime EventDate { get; set; }
    }
}
