using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MyResto2.Models;
using System.Security.Cryptography;
using System.Text;

namespace MyResto2.Controllers
{
    public class LoginController : Controller
    {
        static dbResto2Entities db = new dbResto2Entities();
        
        // GET: Login
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }
        
        // GET: Login/Login
        public ActionResult Login()
        {
            // If user is already logged in, redirect to appropriate dashboard
            if (Session["UserID"] != null)
            {
                string role = Session["UserRole"].ToString();
                if (role == "admin")
                    return RedirectToAction("Dashboard", "Admin");
                else if (role == "cashier")
                    return RedirectToAction("ProcessPayment", "Cashier");
            }
            
            return View();
        }
        
        // POST: Login/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                    // Hash the password for comparison
                    string hashedPassword = HashPassword(model.Password);
                    System.Diagnostics.Debug.WriteLine($"Hashed password: {hashedPassword}");
                    
                    // First check if the username exists
                    var adminByUsername = db.Admins.FirstOrDefault(a => a.username == model.Username);
                    if (adminByUsername == null)
                    {
                        ViewBag.ErrorMessage = "Username not found";
                        return View(model);
                    }
                    
                    // Log the stored hash for comparison
                    System.Diagnostics.Debug.WriteLine($"Stored hash: {adminByUsername.password_hash}");
                    
                    // Now check the password
                    var admin = db.Admins.FirstOrDefault(a => a.username == model.Username && a.password_hash == hashedPassword);
                    
                    if (admin != null)
                    {
                        // Store admin information in session
                        Session["UserID"] = admin.admin_id;
                        Session["Username"] = admin.username;
                        Session["UserRole"] = admin.role;
                        Session["FullName"] = admin.fullname;
                        
                        System.Diagnostics.Debug.WriteLine($"Login successful: Role={admin.role}");
                        
                        // Redirect based on role
                        if (admin.role == "admin")
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else if (admin.role == "cashier")
                        {
                            return RedirectToAction("ProcessPayment", "Cashier");
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Invalid role assigned to user";
                            return View(model);
                        }
                    }
                    else
                    {
                        // Password doesn't match
                        ViewBag.ErrorMessage = "Invalid password";
                        return View(model);
                    }
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View(model);
            }
        }
        
        // GET: Login/Logout
        public ActionResult Logout()
        {
            // Clear session
            Session.Clear();
            Session.Abandon();
            
            // Clear authentication cookie
            FormsAuthentication.SignOut();
            
            return RedirectToAction("Login");
        }
        
        // Hash Password menggunakan SHA-256
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}