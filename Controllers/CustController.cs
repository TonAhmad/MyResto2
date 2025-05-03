using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Newtonsoft.Json;

namespace MyResto2.Controllers
{
    public class CustController : Controller
    {
        static dbResto2Entities db = new dbResto2Entities();
        
        public ActionResult Home()
        {
            // Get top selling products (for now, just get some products)
            // In a real app, you would calculate this based on order quantities
            var topProducts = db.Products
                .Include(p => p.Category)
                .OrderBy(p => Guid.NewGuid()) // Random order for now
                .Take(6)
                .ToList();
                
            // Get all categories for the navbar
            ViewBag.Categories = db.Categories.ToList();
                
            return View(topProducts);
        }
        
        public ActionResult Menu()
        {
            // Get all products with their categories
            var products = db.Products.Include(p => p.Category).ToList();
            
            // Get all categories for filtering
            ViewBag.Categories = db.Categories.ToList();
            
            return View(products);
        }
        
        [HttpPost]
        public JsonResult GetProductStock(string productID)
        {
            var product = db.Products.Find(productID);
            if (product != null)
            {
                return Json(product.stock);
            }
            return Json(0);
        }
        
        public ActionResult Checkout()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmCheckout(string customerName, string cartData, decimal totalAmount)
        {
            if (string.IsNullOrEmpty(cartData))
            {
                return RedirectToAction("Menu");
            }
            
            try
            {
                // Parse cart data
                var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartData);
                
                if (cartItems == null || cartItems.Count == 0)
                {
                    return RedirectToAction("Menu");
                }
                
                // Generate new order ID using current date and time in format ORD(yyyyMMddHHmmss)
                string newOrderId = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");
                
                // Create new order header
                var orderHeader = new OrderHeader
                {
                    orderID = newOrderId,
                    customerName = customerName,
                    orderDate = DateTime.Now,
                    total = totalAmount,
                    orderStatus = "pending" // Initial status
                };
                
                db.OrderHeaders.Add(orderHeader);
                
                // Create order details
                foreach (var item in cartItems)
                {
                    var product = db.Products.Find(item.productID);
                    
                    if (product != null && product.stock >= item.quantity)
                    {
                        decimal subtotal = item.price * item.quantity;
                        
                        // Create order detail
                        var orderDetail = new OrderDetail
                        {
                            orderID = newOrderId,
                            productID = item.productID,
                            quantity = item.quantity,
                            subtotal = subtotal
                        };
                        
                        db.OrderDetails.Add(orderDetail);
                        
                        // Update product stock
                        product.stock -= item.quantity;
                    }
                }
                
                db.SaveChanges();
                
                // Pass order ID to success page
                TempData["OrderID"] = newOrderId;
                return RedirectToAction("SuccessOrder");
            }
            catch (Exception ex)
            {
                // Log error
                ViewBag.ErrorMessage = "Terjadi kesalahan saat memproses pesanan.";
                return View("Checkout");
            }
        }
        
        public ActionResult SuccessOrder()
        {
            string orderId = TempData["OrderID"] as string;
            
            if (string.IsNullOrEmpty(orderId))
            {
                return RedirectToAction("Menu");
            }
            
            var order = db.OrderHeaders
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.orderID == orderId);
                
            if (order == null)
            {
                return RedirectToAction("Menu");
            }
            
            return View(order);
        }
    }
    
    // Helper class for deserializing cart data
    public class CartItem
    {
        public string productID { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
    }
}