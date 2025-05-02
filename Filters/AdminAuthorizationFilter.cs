using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MyResto2.Filters
{
    public class AdminAuthorizationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check if user is logged in
            if (filterContext.HttpContext.Session["UserID"] == null)
            {
                // User is not logged in, redirect to login page
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Login" },
                        { "action", "Login" }
                    });
                return;
            }

            // Check if user has admin role
            string role = filterContext.HttpContext.Session["UserRole"]?.ToString();
            if (filterContext.Controller.GetType().Name == "AdminController" && role != "admin")
            {
                // User doesn't have admin role, redirect to appropriate page
                if (role == "cashier")
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Cashier" },
                            { "action", "ProcessPayment" }
                        });
                }
                else
                {
                    // Fallback to login page
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Login" },
                            { "action", "Login" }
                        });
                }
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}