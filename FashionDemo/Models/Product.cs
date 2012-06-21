using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace FashionDemo.Models
{
    public class Product
    {
        public Product() {
            DateAdded = DateTime.Now;
        }

        [Key]
        public int ProductId { get; set; }
        [Required]
        public string MasterId { get; set; }
        public string Currency { get; set; }
        public string Color { get; set; }
        public string CodeName { get; set; }
        public DateTime DateAdded { get; set; }
        public int Size { get; set; }
        public int Quantity { get; set; }
        public double SellPrice { get; set; }

        public  MasterProduct MasterProduct { get; set; }
    }
}