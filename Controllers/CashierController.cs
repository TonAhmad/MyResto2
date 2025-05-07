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
                    order.admin_id = Session["UserID"].ToString();
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
                // Validate that amount paid is sufficient
                if (amountPaid < order.total)
                {
                    TempData["ErrorMessage"] = "Payment amount must be greater than or equal to the total amount.";
                    return RedirectToAction("SelectOrder", new { orderId = orderId });
                }

                // Calculate change
                decimal change = amountPaid - order.total;
                
                // Update order status to completed
                order.orderStatus = "completed";
                
                // Set the admin_id to the logged-in cashier's ID
                order.admin_id = Session["UserID"].ToString();
                
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Payment processed successfully. Change: Rp " + change.ToString("N0");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid order or order status.";
            }
            return RedirectToAction("ProcessPayment");
        }

        public ActionResult TransactionHistory(DateTime? startDate = null, DateTime? endDate = null, string orderStatus = "all")
        {
            // Set default date range if not provided
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-30);
            
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            
            // Ensure end date includes the entire day
            endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            
            // Store filter values in ViewBag for form
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.OrderStatus = orderStatus;
            
            // Get orders based on filter criteria
            var query = db.OrderHeaders.AsQueryable();
            
            // Apply date filter
            query = query.Where(o => o.orderDate >= startDate && o.orderDate <= endDate);
            
            // Apply status filter
            if (orderStatus != "all")
            {
                query = query.Where(o => o.orderStatus == orderStatus);
            }
            else
            {
                // If "all" is selected, only show completed and cancelled orders
                query = query.Where(o => o.orderStatus == "completed" || o.orderStatus == "cancelled");
            }
            
            // Execute query and get results
            var orders = query.OrderByDescending(o => o.orderDate).ToList();
            
            // Map to view model
            var orderSummaries = orders.Select(o => new Models.OrderSummary
            {
                OrderID = o.orderID,
                CustomerName = o.customerName,
                OrderDate = o.orderDate ?? DateTime.MinValue,
                Total = o.total,
                OrderStatus = o.orderStatus,
                CashierID = o.admin_id,
                CashierName = o.Admin != null ? o.Admin.fullname : "Unknown"
            }).ToList();
            
            // Calculate summary statistics
            var totalTransactions = orderSummaries.Count;
            var totalSales = orderSummaries.Where(o => o.OrderStatus == "completed").Sum(o => o.Total);
            var averageOrderValue = totalTransactions > 0 ? 
                orderSummaries.Where(o => o.OrderStatus == "completed").DefaultIfEmpty().Average(o => o?.Total ?? 0) : 0;
            
            // Create view model
            var viewModel = new TransactionHistoryViewModel
            {
                Orders = orderSummaries,
                TotalTransactions = totalTransactions,
                TotalSales = totalSales,
                AverageOrderValue = averageOrderValue
            };
            
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult GetOrderDetails(string orderId)
        {
            var order = db.OrderHeaders.Find(orderId);
            if (order == null)
            {
                return PartialView("_OrderDetails", new OrderDetailsViewModel
                {
                    OrderHeader = new OrderSummary(),
                    OrderDetails = new List<OrderItemDetail>()
                });
            }
            
            // Get order details
            var orderDetails = db.OrderDetails
                .Where(od => od.orderID == orderId)
                .ToList();
            
            // Map to view model
            var orderHeader = new OrderSummary
            {
                OrderID = order.orderID,
                CustomerName = order.customerName,
                OrderDate = order.orderDate ?? DateTime.MinValue,
                Total = order.total,
                OrderStatus = order.orderStatus,
                CashierID = order.admin_id,
                CashierName = order.Admin != null ? order.Admin.fullname : "Unknown"
            };
            
            var orderItems = orderDetails.Select(od => new OrderItemDetail
            {
                ProductID = od.productID,
                ProductName = od.Product.productName,
                UnitPrice = od.Product.price,
                Quantity = od.quantity,
                Subtotal = od.subtotal ?? 0
            }).ToList();
            
            var viewModel = new OrderDetailsViewModel
            {
                OrderHeader = orderHeader,
                OrderDetails = orderItems
            };
            
            return PartialView("_OrderDetails", viewModel);
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
