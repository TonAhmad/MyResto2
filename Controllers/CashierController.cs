using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyResto2.Controllers
{
    public class CashierController : Controller
    {
        static dbResto2Entities db = new dbResto2Entities();
        public ActionResult ProcessPayment()
        {
            return View();
        }
    }
}