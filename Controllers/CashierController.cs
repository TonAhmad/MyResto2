using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyResto2.Filters;
using MyResto2.Models;

namespace MyResto2.Controllers
{
    [CashierAuthorizationFilter]
    public class CashierController : Controller
    {
        private dbResto2Entities db = new dbResto2Entities();

        // GET: Cashier/ProcessPayment
        public ActionResult ProcessPayment()
        {
            // Get pending orders
            var pendingOrdersFromDb = db.OrderHeaders
                .Where(o => o.orderStatus == "pending")
                .OrderByDescending(o => o.orderDate)
                .ToList();

            // Convert to our model OrderHeader
            var pendingOrders = pendingOrdersFromDb.Select(o => new Models.OrderHeader
            {
                orderID = o.orderID,
                customerName = o.customerName,
                orderDate = o.orderDate,
                orderStatus = o.orderStatus,
                total = o.total,
                admin_id = o.admin_id
            }).ToList();

            // Get confirmed orders
            var confirmedOrdersFromDb = db.OrderHeaders
                .Where(o => o.orderStatus == "confirmed")
                .OrderByDescending(o => o.orderDate)
                .ToList();

            // Convert to our model OrderHeader
            var confirmedOrders = confirmedOrdersFromDb.Select(o => new Models.OrderHeader
            {
                orderID = o.orderID,
                customerName = o.customerName,
                orderDate = o.orderDate,
                orderStatus = o.orderStatus,
                total = o.total,
                admin_id = o.admin_id
            }).ToList();

            // Create view model
            var viewModel = new ProcessPaymentViewModel
            {
                PendingOrders = pendingOrders,
                ConfirmedOrders = confirmedOrders
            };

            return View(viewModel);
        }

        // POST: Cashier/ConfirmOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmOrder(string orderId)
        {
            try
            {
                // Debug the incoming orderId
                System.Diagnostics.Debug.WriteLine($"Confirming order: {orderId}");
                
                var order = db.OrderHeaders.Find(orderId);
                
                if (order == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Order not found: {orderId}");
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction("ProcessPayment");
                }
                
                System.Diagnostics.Debug.WriteLine($"Order status: {order.orderStatus}");
                
                if (order.orderStatus == "pending")
                {
                    order.orderStatus = "confirmed";
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Order confirmed successfully.";
                    System.Diagnostics.Debug.WriteLine("Order confirmed successfully");
                }
                else
                {
                    TempData["ErrorMessage"] = "Order is not in pending status.";
                    System.Diagnostics.Debug.WriteLine($"Order is not in pending status: {order.orderStatus}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error confirming order: {ex.Message}");
                TempData["ErrorMessage"] = $"Error confirming order: {ex.Message}";
            }
            
            return RedirectToAction("ProcessPayment");
        }

        // POST: Cashier/SelectOrder
        [HttpPost]
        public ActionResult SelectOrder(string orderId)
        {
            var order = db.OrderHeaders.Find(orderId);
            if (order != null)
            {
                var orderDetails = db.OrderDetails
                    .Where(od => od.orderID == orderId)
                    .ToList();

                var orderDetailsWithProducts = orderDetails.Select(od => new OrderDetailViewModel
                {
                    OrderID = od.orderID,
                    ProductID = od.productID,
                    ProductName = od.Product.productName,
                    Quantity = od.quantity,
                    Subtotal = od.subtotal ?? 0,
                    CustomerName = order.customerName
                }).ToList();

                // Get pending orders
                var pendingOrdersFromDb = db.OrderHeaders
                    .Where(o => o.orderStatus == "pending")
                    .OrderByDescending(o => o.orderDate)
                    .ToList();

                // Convert to our model OrderHeader
                var pendingOrders = pendingOrdersFromDb.Select(o => new Models.OrderHeader
                {
                    orderID = o.orderID,
                    customerName = o.customerName,
                    orderDate = o.orderDate,
                    orderStatus = o.orderStatus,
                    total = o.total,
                    admin_id = o.admin_id
                }).ToList();

                // Get confirmed orders
                var confirmedOrdersFromDb = db.OrderHeaders
                    .Where(o => o.orderStatus == "confirmed")
                    .OrderByDescending(o => o.orderDate)
                    .ToList();

                // Convert to our model OrderHeader
                var confirmedOrders = confirmedOrdersFromDb.Select(o => new Models.OrderHeader
                {
                    orderID = o.orderID,
                    customerName = o.customerName,
                    orderDate = o.orderDate,
                    orderStatus = o.orderStatus,
                    total = o.total,
                    admin_id = o.admin_id
                }).ToList();

                // In the SelectOrder method
                var viewModel = new ProcessPaymentViewModel
                {
                    PendingOrders = pendingOrders,
                    ConfirmedOrders = confirmedOrders,
                    SelectedOrderDetails = orderDetailsWithProducts,
                    TotalAmount = order.total
                };

                return View("ProcessPayment", viewModel);
            }
            return RedirectToAction("ProcessPayment");
        }

        // POST: Cashier/ProcessPayment
        [HttpPost]
        public ActionResult ProcessPayment(string orderId, decimal amountPaid)
        {
            var order = db.OrderHeaders.Find(orderId);
            if (order != null && order.orderStatus == "confirmed")
            {
                // Update order status to completed
                order.orderStatus = "completed";
                
                // Set the admin_id to the logged-in cashier's ID
                order.admin_id = Session["UserID"].ToString();
                
                db.SaveChanges();
                
                // In the ProcessPayment method
                // Calculate change
                decimal change = amountPaid - order.total;
                
                TempData["SuccessMessage"] = "Payment processed successfully. Change: Rp " + change.ToString("N0");
            }
            return RedirectToAction("ProcessPayment");
        }

        // Helper method to pass user info to all views
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
    }
}
