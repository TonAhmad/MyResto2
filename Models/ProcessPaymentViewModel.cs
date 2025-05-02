using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyResto2.Models
{
    public class ProcessPaymentViewModel
    {
        public List<OrderHeader> PendingOrders { get; set; }
        public List<OrderHeader> ConfirmedOrders { get; set; }
        public List<OrderDetailViewModel> SelectedOrderDetails { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }
        public string SelectedOrderId { get; set; }

        public ProcessPaymentViewModel()
        {
            PendingOrders = new List<OrderHeader>();
            ConfirmedOrders = new List<OrderHeader>();
            SelectedOrderDetails = new List<OrderDetailViewModel>();
        }
    }

    // This class represents your database OrderHeader entity
    public class OrderHeader
    {
        public string orderID { get; set; }
        public string customerName { get; set; }
        public DateTime? orderDate { get; set; }
        public string orderStatus { get; set; }
        public decimal? total { get; set; }
        public string admin_id { get; set; }
    }

    public class OrderDetailViewModel
    {
        public string OrderID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string CustomerName { get; set; }
    }
}