using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyResto2.Models
{
    public class OrderViewModel
    {
        public string CustomerName { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}