using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FashionDemo.Models;

namespace FashionDemo.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Product> Products { get; set; }

    }
}