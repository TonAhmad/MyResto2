using System;
using System.Linq;
using System.Web.Mvc;
using MyResto2.Models; // Ganti sesuai namespace model kamu

namespace MyResto2.Controllers
{
    public class CashierController : Controller
    {
        static dbResto2Entities db = new dbResto2Entities();

        // GET: Cashier/ProcessPayment
        public ActionResult ProcessPayment()
        {
            return View();
        }

        // POST: Cashier/ProcessPayment
        [HttpPost]
        public ActionResult ProcessPayment(OrderHeader order)
        {
            order.orderDate = DateTime.Now;
            order.orderStatus = "completed";
            // Simpan ID kasir yang sedang login (contoh hardcode, nanti bisa pakai session)
            order.admin_id = "ADM001";

            db.OrderHeader.Add(order);
            db.SaveChanges();

            return RedirectToAction("TransactionHistory");
        }

        // GET: Cashier/TransactionHistory
        public ActionResult TransactionHistory()
        {
            var list = db.OrderHeader
                         .Where(o => o.orderStatus == "completed")
                         .OrderByDescending(o => o.orderDate)
                         .ToList();

            return View(list);
        }

        // GET: Cashier/TransactionDetail/{id}
        public ActionResult TransactionDetail(string id)
        {
            var transaksi = db.OrderHeader.FirstOrDefault(t => t.orderID == id);
            if (transaksi == null)
                return HttpNotFound();

            var detail = db.OrderDetail
                           .Where(d => d.orderID == id)
                           .ToList();

            ViewBag.DetailItems = detail;
            return View(transaksi);
        }
    }
}
