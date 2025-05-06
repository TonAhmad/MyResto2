using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using MyResto2.Filters;

namespace MyResto2.Controllers
{
    [AdminAuthorizationFilter]
    public class AdminController : Controller
    {
        static dbResto2Entities db = new dbResto2Entities();

        // Hash Password menggunakan SHA-256
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
        public ActionResult Dashboard()
        {
            // Get statistics for dashboard
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalProducts = db.Products.Count();
            
            // Get total transactions (orders) - only completed orders
            ViewBag.TotalTransactions = db.OrderHeaders
                .Where(o => o.orderStatus == "completed")
                .Count();
            
            // Get total revenue - handle null values and only count completed orders
            decimal totalRevenue = 0;
            try
            {
                totalRevenue = db.OrderHeaders
                    .Where(o => o.orderStatus == "completed")
                    .Sum(o => o.total);
            }
            catch (InvalidOperationException)
            {
                // This will catch the exception if there are no completed orders
                totalRevenue = 0;
            }
            
            ViewBag.TotalRevenue = totalRevenue.ToString("N0");
            
            // Get products with low stock (<=10)
            var lowStockProducts = db.Products
                .Where(p => p.stock <= 10)
                .Include(p => p.Category)
                .OrderBy(p => p.stock)
                .ToList();
            
            return View(lowStockProducts);
        }

        // Menampilkan halaman ManageCashier dengan daftar admin dan cashier
        public ActionResult ManageCashier()
        {
            var adminList = db.Admins.ToList();
            return View(adminList);
        }

        // Menampilkan form untuk menambah admin/cashier baru
        public ActionResult AddAdmin()
        {
            return View();
        }

        // Menyimpan data admin/cashier baru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                // Validasi role hanya admin atau cashier
                if (admin.role != "admin" && admin.role != "cashier")
                {
                    ModelState.AddModelError("role", "Role hanya boleh admin atau cashier");
                    return View(admin);
                }

                // Generate ID baru (ADM + 3 digit angka)
                var lastAdmin = db.Admins.OrderByDescending(a => a.admin_id).FirstOrDefault();
                string newId = "ADM001";

                if (lastAdmin != null)
                {
                    string lastId = lastAdmin.admin_id;
                    if (lastId.StartsWith("ADM"))
                    {
                        int lastNumber = int.Parse(lastId.Substring(3));
                        newId = $"ADM{(lastNumber + 1).ToString("D3")}"; // Format 3 digit
                    }
                }

                admin.admin_id = newId;
                admin.created_at = DateTime.Now;

                // Hash password
                admin.password_hash = HashPassword(admin.password_hash);

                db.Admins.Add(admin);
                db.SaveChanges();
                return RedirectToAction("ManageCashier");
            }

            return View(admin);
        }

        // Menampilkan form untuk edit admin/cashier
        public ActionResult EditAdmin(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }

            return View(admin);
        }

        // Menyimpan perubahan data admin/cashier
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                // Validasi role hanya admin atau cashier
                if (admin.role != "admin" && admin.role != "cashier")
                {
                    ModelState.AddModelError("role", "Role hanya boleh admin atau cashier");
                    return View(admin);
                }

                // Ambil data admin yang ada di database
                var existingAdmin = db.Admins.Find(admin.admin_id);
                
                if (existingAdmin != null)
                {
                    // Update properti yang diubah
                    existingAdmin.fullname = admin.fullname;
                    existingAdmin.username = admin.username;
                    existingAdmin.email = admin.email;
                    existingAdmin.phone_number = admin.phone_number;
                    existingAdmin.address = admin.address;
                    existingAdmin.role = admin.role;
                    
                    // Jika password kosong, gunakan password lama (tidak perlu diubah)
                    if (!string.IsNullOrEmpty(admin.password_hash))
                    {
                        // Jika password diubah, hash password baru
                        existingAdmin.password_hash = HashPassword(admin.password_hash);
                    }
                    
                    // Simpan perubahan
                    db.SaveChanges();
                    return RedirectToAction("ManageCashier");
                }
            }

            return View(admin);
        }

        // Menampilkan konfirmasi hapus admin/cashier
        public ActionResult DeleteAdmin(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }

            return View(admin);
        }

        // Menghapus data admin/cashier
        [HttpPost, ActionName("DeleteAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAdminConfirmed(string id)
        {
            Admin admin = db.Admins.Find(id);
            db.Admins.Remove(admin);
            db.SaveChanges();
            return RedirectToAction("ManageCashier");
        }

        // Menampilkan halaman ManageProducts dengan daftar produk dan kategori
        public ActionResult ManageProducts()
        {
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Products = db.Products.Include(p => p.Category).ToList();
            return View();
        }

        #region Category Management

        // Menampilkan form untuk menambah kategori baru
        public ActionResult AddCategory()
        {
            return View();
        }

        // Menyimpan data kategori baru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // Generate ID baru (CAT + 3 digit angka)
                var lastCategory = db.Categories.OrderByDescending(c => c.categoryID).FirstOrDefault();
                string newId = "CAT001";

                if (lastCategory != null)
                {
                    string lastId = lastCategory.categoryID;
                    if (lastId.StartsWith("CAT"))
                    {
                        int lastNumber = int.Parse(lastId.Substring(3));
                        newId = $"CAT{(lastNumber + 1).ToString("D3")}"; // Format 3 digit
                    }
                }

                category.categoryID = newId;
                category.createdAt = DateTime.Now;

                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("ManageProducts");
            }

            return View(category);
        }

        // Menampilkan form untuk edit kategori
        public ActionResult EditCategory(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // Menyimpan perubahan data kategori
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = db.Categories.Find(category.categoryID);
                
                if (existingCategory != null)
                {
                    existingCategory.categoryName = category.categoryName;
                    
                    db.SaveChanges();
                    return RedirectToAction("ManageProducts");
                }
            }

            return View(category);
        }

        // Menampilkan konfirmasi hapus kategori
        public ActionResult DeleteCategory(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }

        // Menghapus data kategori
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategoryConfirmed(string id)
        {
            Category category = db.Categories.Find(id);
            
            // Check if category has products
            if (category.Products.Count > 0)
            {
                ModelState.AddModelError("", "Kategori ini memiliki produk terkait dan tidak dapat dihapus.");
                return View("DeleteCategory", category);
            }
            
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("ManageProducts");
        }
        
        // AJAX method to get all categories
        public JsonResult GetCategories()
        {
            var categories = db.Categories.Select(c => new { 
                c.categoryID, 
                c.categoryName, 
                c.createdAt,
                ProductCount = c.Products.Count
            }).ToList();
            
            return Json(categories, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Product Management

        // Menampilkan form untuk menambah produk baru
        public ActionResult AddProduct()
        {
            ViewBag.Categories = db.Categories.ToList();
            return View();
        }

        // Menyimpan data produk baru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(Product product, HttpPostedFileBase productImage)
        {
            ViewBag.Categories = db.Categories.ToList();

            if (ModelState.IsValid)
            {
                // Generate ID baru (PRD + 3 digit angka)
                var lastProduct = db.Products.OrderByDescending(p => p.productID).FirstOrDefault();
                string newId = "PRD001";

                if (lastProduct != null)
                {
                    string lastId = lastProduct.productID;
                    if (lastId.StartsWith("PRD"))
                    {
                        int lastNumber = int.Parse(lastId.Substring(3));
                        newId = $"PRD{(lastNumber + 1).ToString("D3")}"; // Format 3 digit
                    }
                }

                product.productID = newId;
                product.createdAt = DateTime.Now;

                // Handle image upload
                if (productImage != null && productImage.ContentLength > 0)
                {
                    // Create directory if it doesn't exist
                    string uploadDirectory = Server.MapPath("~/ProductImages");
                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory);
                    }

                    // Generate unique filename
                    string fileName = $"{newId}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{Path.GetExtension(productImage.FileName)}";
                    string filePath = Path.Combine(uploadDirectory, fileName);
                    
                    // Save file
                    productImage.SaveAs(filePath);
                    
                    // Save relative path to database
                    product.imagePath = $"/ProductImages/{fileName}";
                }
                else
                {
                    // Set default image if no image is uploaded
                    product.imagePath = "/ProductImages/default-product.jpg";
                }

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("ManageProducts");
            }

            return View(product);
        }

        // Menampilkan form untuk edit produk
        public ActionResult EditProduct(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categories = db.Categories.ToList();
            return View(product);
        }

        // Menyimpan perubahan data produk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(Product product, HttpPostedFileBase productImage)
        {
            ViewBag.Categories = db.Categories.ToList();

            if (ModelState.IsValid)
            {
                var existingProduct = db.Products.Find(product.productID);
                
                if (existingProduct != null)
                {
                    // Update properties
                    existingProduct.productName = product.productName;
                    existingProduct.categoryID = product.categoryID;
                    existingProduct.price = product.price;
                    existingProduct.stock = product.stock;
                    existingProduct.description = product.description;
                    
                    // Handle image upload if a new image is provided
                    if (productImage != null && productImage.ContentLength > 0)
                    {
                        // Create directory if it doesn't exist
                        string uploadDirectory = Server.MapPath("~/ProductImages");
                        if (!Directory.Exists(uploadDirectory))
                        {
                            Directory.CreateDirectory(uploadDirectory);
                        }

                        // Generate unique filename
                        string fileName = $"{product.productID}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{Path.GetExtension(productImage.FileName)}";
                        string filePath = Path.Combine(uploadDirectory, fileName);
                        
                        // Delete old image if it exists and is not the default
                        if (!string.IsNullOrEmpty(existingProduct.imagePath) && 
                            existingProduct.imagePath != "/ProductImages/default-product.jpg" &&
                            System.IO.File.Exists(Server.MapPath($"~{existingProduct.imagePath}")))
                        {
                            System.IO.File.Delete(Server.MapPath($"~{existingProduct.imagePath}"));
                        }
                        
                        // Save new file
                        productImage.SaveAs(filePath);
                        
                        // Save relative path to database
                        existingProduct.imagePath = $"/ProductImages/{fileName}";
                    }
                    
                    db.SaveChanges();
                    return RedirectToAction("ManageProducts");
                }
            }

            return View(product);
        }

        // Menampilkan konfirmasi hapus produk
        public ActionResult DeleteProduct(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // Menghapus data produk
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProductConfirmed(string id)
        {
            Product product = db.Products.Find(id);
            
            // Check if product has order details
            if (product.OrderDetails.Count > 0)
            {
                ModelState.AddModelError("", "Produk ini memiliki riwayat pesanan dan tidak dapat dihapus.");
                return View("DeleteProduct", product);
            }
            
            // Delete product image if it exists and is not the default
            if (!string.IsNullOrEmpty(product.imagePath) && 
                product.imagePath != "/ProductImages/default-product.jpg" &&
                System.IO.File.Exists(Server.MapPath($"~{product.imagePath}")))
            {
                System.IO.File.Delete(Server.MapPath($"~{product.imagePath}"));
            }
            
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("ManageProducts");
        }

        #endregion

        // Add this to all action methods to pass the username to the view
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            
            // Pass the username to all views
            if (Session["Username"] != null)
            {
                ViewBag.Username = Session["Username"].ToString();
                ViewBag.FullName = Session["FullName"]?.ToString();
            }
        }
        public ActionResult Reports(DateTime? startDate = null, DateTime? endDate = null, string orderStatus = null)
        {
            // Set default date range if not provided (last 30 days)
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-30);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Adjust end date to include the entire day
            endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            
            // Get orders based on filter
            var query = db.OrderHeaders.AsQueryable();
            
            // Apply date filter
            query = query.Where(o => o.orderDate >= startDate && o.orderDate <= endDate);
            
            // Apply status filter if provided
            if (!string.IsNullOrEmpty(orderStatus))
            {
                query = query.Where(o => o.orderStatus == orderStatus);
            }
            
            // Include related data and order by date
            var orders = query
                .Include(o => o.Admin)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .OrderByDescending(o => o.orderDate)
                .ToList();
            
            // Calculate summary statistics
            ViewBag.TotalOrders = orders.Count;
            ViewBag.TotalRevenue = orders.Sum(o => o.total);
            ViewBag.CompletedOrders = orders.Count(o => o.orderStatus == "completed");
            ViewBag.PendingOrders = orders.Count(o => o.orderStatus == "pending");
            
            // Pass filter values to view for maintaining state
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.OrderStatus = orderStatus;
            
            return View(orders);
        }
    }
}