
using System.Collections.Generic;
using System.Data.Entity;

namespace FashionDemo.Models
{
    public class FashionDb:DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<MasterProduct> MasterProducts { get; set; }

        public FashionDb()
        {
            Configuration.LazyLoadingEnabled = true;
        }
    }
}