using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MyResto2.Filters
{
    public class CashierAuthorizationFilter : ActionFilterAttribute
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

            // Check if user has cashier role
            string role = filterContext.HttpContext.Session["UserRole"]?.ToString();
            if (filterContext.Controller.GetType().Name == "CashierController" && role != "cashier")
            {
                // User doesn't have cashier role, redirect to appropriate page
                if (role == "admin")
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                            { "controller", "Admin" },
                            { "action", "Dashboard" }
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