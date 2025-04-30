using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyResto2.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public string CustomerName { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<TransactionItem> Items { get; set; }
    }

    public class TransactionItem
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

}