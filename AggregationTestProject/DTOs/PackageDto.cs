using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AggregationTestProject.DTOs
{
    public class PackageDto
    {
        public string Volume { get; set; }
        public int BoxFormat { get; set; }
        public int? BoxFormatItemsPerLayer { get; set; }
        public int? BoxFormatLayersQuantity { get; set; }
        public int ParentBoxFormat { get; set; }
        public string BoxCode { get; set; }
        public bool? IsBoxCodeEan13 { get; set; }
        public int PalletFormat { get; set; }
    }
}
