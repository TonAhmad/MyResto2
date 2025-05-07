using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyResto2.Models
{
    public class TransactionHistoryViewModel
    {
        public List<OrderSummary> Orders { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalSales { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class OrderSummary
    {
        public string OrderID { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public string OrderStatus { get; set; }
        public string CashierID { get; set; }
        public string CashierName { get; set; }
    }

    public class OrderDetailsViewModel
    {
        public OrderSummary OrderHeader { get; set; }
        public List<OrderItemDetail> OrderDetails { get; set; }
    }

    public class OrderItemDetail
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}