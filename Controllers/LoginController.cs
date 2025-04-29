using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyResto2.Controllers
{
    public class LoginController : Controller
    {
        static dbResto2Entities db = new dbResto2Entities();
        public ActionResult Index()
        {
            return View();
        }
    }
}