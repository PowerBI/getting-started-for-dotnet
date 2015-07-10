using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIGettingStarted
{
    //Power BI Datasets
    public class PBIDatasets
    {
        public Dataset[] Datasets { get; set; }
    }

    public class Dataset
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public bool IsCompete { get; set; }
        public DateTime ManufacturedOn { get; set; }
    }
}
