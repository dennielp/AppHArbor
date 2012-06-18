using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using FashionDemo.Models;
using FashionDemo.ViewModels;

namespace FashionDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var db = new FashionDb();
            
            GetIndexPageData();

            ViewBag.Images = GetImages();
            
            return View(db.MasterProducts.Include("Products").OrderBy(m=>m.MasterId));
        }

        public ActionResult AddMasterProduct(FormCollection form)
        {
            var db = new FashionDb();
            var master = new MasterProduct()
                             {
                                 MasterId = form["masterId"],
                                 Label = form["label"],
                                 Supplier = form["supplier"],
                                 Name = form["name"],
                                 Type = form["productType"]
                             };
            
            var dateAdded = DateTime.Now;

            var productsList = new List<Product>();
            for (int i = Int32.Parse(form["fromSize"]); i <= Int32.Parse(form["toSize"]); i += 2)
            {
                productsList.Add(new Product()
                                     {
                                         Price = Double.Parse(form["price"]),
                                         Color = form["color"],
                                         Currency = form["currency"],
                                         DateAdded = dateAdded,
                                         MasterId = master.MasterId,
                                         Size = i,
                                         Quantity = Int32.Parse(form["quantity"]),
                                         CodeName = String.Format("{0}-{1}-{2}",master.MasterId,i,form["color"])
                                     });
            }
            master.Products = productsList;
            db.MasterProducts.Add(master);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        
        public ActionResult Search(string searchFor)
        {
            if(!String.IsNullOrEmpty(searchFor))
            {
                var db = new FashionDb();
                ViewBag.Images = GetImages();
                GetIndexPageData();
                return View("Index", db.MasterProducts.Include("Products").Where(m => m.Name.Contains(searchFor) || m.Label.Contains(searchFor)));    
            }
            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            var images = new List<string>();
            var d = new DirectoryInfo(Server.MapPath("~/Uploads/Images"));

            var imageFiles = d.GetFiles();
            foreach (var file in imageFiles.Where(f=>f.Name.Contains("_refl")))
            {
                images.Add("~/Uploads/Images/"+file.Name);
            }
            GetIndexPageData();
            ViewBag.images = images;
            return View();
        }

        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                string filePath = Path.Combine(Server.MapPath("~/Uploads/Images"),
                                               Path.GetFileName(file.FileName));
                file.SaveAs(filePath);
                MakeReflectionImage(filePath);
            }
            
            
            return RedirectToAction("Index");
        }

        private void GetIndexPageData()
        {
            var labels = new List<string>()
                             {
                                 "Gucci",
                                 "Armani"
                             }.Select(l=>new SelectListItem()
                                             {
                                                 Text = l,
                                                 Value = l
                                             });
            ViewBag.label = labels;
            var prodTypes = new List<string>()
                                {
                                    "T-shirt",
                                    "Top",
                                    "Jeans"
                                }.Select(p=>new SelectListItem()
                                                {
                                                    Text = p,
                                                    Value = p
                                                });
            ViewBag.productType = prodTypes;

            var sizes = new List<int>();
            for (int i = 8; i <= 20; i+=2)
            {
                sizes.Add(i);
            }
            ViewBag.fromSize=ViewBag.toSize = sizes.Select(s => new SelectListItem()
                                                 {
                                                     Text = s.ToString(),
                                                     Value = s.ToString()
                             
                                                 });
            var curs = new List<string>()
                           {
                               "$",
                               "€",
                               "£"
                           }.Select(c=>new SelectListItem()
                                           {
                                               Text = c,
                                               Value = c
                                           });
            ViewBag.currency = curs;
            var colors = new List<string>()
                              {
                                  "Black",
                                  "White",
                                  "Red",
                                  "Blue",
                                  "Green"
                              }.Select(col => new SelectListItem()
                                                  {
                                                      Value = col,
                                                      Text = col
                                                  });
            ViewBag.color = colors;
        }

        
        public JsonResult GetProductById(int id)
        {
            var db = new FashionDb();
            var prod = db.Products.Where(p => p.ProductId == id).Single();
            var master = db.MasterProducts.Where(m => m.MasterId == prod.MasterId).Single();
            return Json(new
                            {
                                prod.ProductId,
                                prod.Price,
                                prod.MasterId,
                                prod.Currency,
                                prod.Color,
                                prod.Quantity,
                                prod.CodeName,
                                prod.MasterProduct.Name,
                                prod.MasterProduct.Label,
                                prod.MasterProduct.Supplier,
                                prod.MasterProduct.Type
                                
                            }, JsonRequestBehavior.AllowGet);

        }

        public void SaveChildChanges(FormCollection collection)
        {
            var db = new FashionDb();
            var id = Int32.Parse(collection["hdnChildId"]);
            var price = Int32.Parse(collection["txtEditPrice"]);
            var q = Int32.Parse(collection["txtEditQuantity"]);

            var prod = db.Products.Where(p => p.ProductId == id).Single();
            prod.Price = price;
            prod.Quantity = q;

            db.SaveChanges();
            return;
        }

        public void DeleteProduct(int id)
        {
            var db = new FashionDb();
            var prod=db.Products.Where(p => p.ProductId == id).Single();
            db.Products.Remove(prod);
            db.SaveChanges();
        }

        public ActionResult SaveMasterChanges(FormCollection form)
        {
            var db = new FashionDb();
            var label = form["label"];
            var type = form["productType"];
            var name = form["name"];
            var masterId = GetMasterId(form["masterId"]);
            

            var master = db.MasterProducts.Where(m => m.MasterId == masterId).Single();
            master.Label = label;
            master.Type = type;
            master.Name = name;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public void DeleteMaster(string id)
        {
            var db = new FashionDb();
            var masterId = GetMasterId(id);

            var master = db.MasterProducts.Where(m => m.MasterId == masterId).Single();
            db.MasterProducts.Remove(master);
            db.SaveChanges();
        }

        private string GetMasterId(string str)
        {
            var masterString = str.Split('-');
            string masterId = string.Empty;
            for (int i = 0; i <= masterString.Count() - 3; i++)
            {
                masterId += masterString[i] += "-";
            }
            masterId = masterId.TrimEnd('-');
            return masterId;
        }

        private List<string> GetImages()
        {
            //getting images
            var d = new DirectoryInfo(Server.MapPath("~/Uploads/Images"));

            var imageFiles = d.GetFiles();
            var images = imageFiles.Where(f => f.Name.Contains("_refl")).Select(file => "~/Uploads/Images/" + file.Name).ToList();
            return images;
        } 

        #region imageProcessing
        private static void MakeReflectionImage(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var img = Image.FromFile(filePath);

            var newImg = DrawReflection(img, Color.Black, 100);
           
            newImg.Save(String.Format("{0}/{1}_refl{2}",fileInfo.DirectoryName,fileInfo.Name,fileInfo.Extension));
            img.Dispose();
            newImg.Dispose();
        }
        private static Image cropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            Bitmap bmpCrop = bmpImage.Clone(cropArea,
            bmpImage.PixelFormat);
            return (Image)(bmpCrop);
        }
        public static Image DrawReflection(Image _Image, Color _BackgroundColor, int _Reflectivity)
        {
            // Calculate the size of the new image
            int height = (int)(_Image.Height + (_Image.Height * ((float)_Reflectivity / 255)));
            Bitmap newImage = new Bitmap(_Image.Width, height, PixelFormat.Format24bppRgb);
            newImage.SetResolution(_Image.HorizontalResolution, _Image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                // Initialize main graphics buffer
                graphics.Clear(_BackgroundColor);
                graphics.DrawImage(_Image, new Point(0, 0));
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                Rectangle destinationRectangle = new Rectangle(0, _Image.Size.Height,
                                                 _Image.Size.Width, _Image.Size.Height);

                // Prepare the reflected image
                int reflectionHeight = (_Image.Height * _Reflectivity) / 255;
                Image reflectedImage = new Bitmap(_Image.Width, reflectionHeight);

                // Draw just the reflection on a second graphics buffer
                using (Graphics gReflection = Graphics.FromImage(reflectedImage))
                {
                    gReflection.DrawImage(_Image,
                       new Rectangle(0, 0, reflectedImage.Width, reflectedImage.Height),
                       0, _Image.Height - reflectedImage.Height, reflectedImage.Width,
                       reflectedImage.Height, GraphicsUnit.Pixel);
                }
                reflectedImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle imageRectangle =
                    new Rectangle(destinationRectangle.X, destinationRectangle.Y,
                    destinationRectangle.Width,
                    (destinationRectangle.Height * _Reflectivity) / 255);

                // Draw the image on the original graphics
                graphics.DrawImage(reflectedImage, imageRectangle);

                // Finish the reflection using a gradiend brush
                LinearGradientBrush brush = new LinearGradientBrush(imageRectangle,
                       Color.FromArgb(255 - _Reflectivity, _BackgroundColor),
                        _BackgroundColor, 90, false);
                graphics.FillRectangle(brush, imageRectangle);
            }

            return newImage;
        }
        #endregion
    }
}
