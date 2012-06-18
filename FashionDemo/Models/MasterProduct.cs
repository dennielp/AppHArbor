using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using FashionDemo.Models;

namespace FashionDemo.Models
{
    public class MasterProduct
    {
        public MasterProduct()
        {
            Products=new List<Product>();
        }
        [Key]
        public string MasterId { get; set; }
        public string Supplier { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public double? Discount { get; set; }
        
        public ICollection<Product> Products { get; set; }
    }
}