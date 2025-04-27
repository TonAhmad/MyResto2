using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyResto2.Controllers
{
    public class CustController : Controller
    {
        static dbResto2Entities koneksi = new dbResto2Entities();
        public ActionResult Home()
        {
            return View();
        }
    }
}