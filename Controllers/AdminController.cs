using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;


namespace MyResto2.Controllers
{
    public class AdminController : Controller
    {
        static dbResto2Entities db = new dbResto2Entities();

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
        public ActionResult Dashboard()
        {
            return View();
        }

        // Menampilkan halaman ManageCashier dengan daftar admin dan cashier
        public ActionResult ManageCashier()
        {
            var adminList = db.Admins.ToList();
            return View(adminList);
        }

        // Menampilkan form untuk menambah admin/cashier baru
        public ActionResult AddAdmin()
        {
            return View();
        }

        // Menyimpan data admin/cashier baru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                // Validasi role hanya admin atau cashier
                if (admin.role != "admin" && admin.role != "cashier")
                {
                    ModelState.AddModelError("role", "Role hanya boleh admin atau cashier");
                    return View(admin);
                }

                // Generate ID baru (ADM + 3 digit angka)
                var lastAdmin = db.Admins.OrderByDescending(a => a.admin_id).FirstOrDefault();
                string newId = "ADM001";

                if (lastAdmin != null)
                {
                    string lastId = lastAdmin.admin_id;
                    if (lastId.StartsWith("ADM"))
                    {
                        int lastNumber = int.Parse(lastId.Substring(3));
                        newId = $"ADM{(lastNumber + 1).ToString("D3")}"; // Format 3 digit
                    }
                }

                admin.admin_id = newId;
                admin.created_at = DateTime.Now;
                
                // Hash password (dalam contoh ini menggunakan BCrypt)
                admin.password_hash = HashPassword(admin.password_hash);

                db.Admins.Add(admin);
                db.SaveChanges();
                return RedirectToAction("ManageCashier");
            }

            return View(admin);
        }

        // Menampilkan form untuk edit admin/cashier
        public ActionResult EditAdmin(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }

            return View(admin);
        }

        // Menyimpan perubahan data admin/cashier
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAdmin(Admin admin)
        {
            if (ModelState.IsValid)
            {
                // Validasi role hanya admin atau cashier
                if (admin.role != "admin" && admin.role != "cashier")
                {
                    ModelState.AddModelError("role", "Role hanya boleh admin atau cashier");
                    return View(admin);
                }

                // Jika password diubah, hash password baru
                var existingAdmin = db.Admins.AsNoTracking().FirstOrDefault(a => a.admin_id == admin.admin_id);
                if (existingAdmin != null && admin.password_hash != existingAdmin.password_hash)
                {
                    admin.password_hash = HashPassword(admin.password_hash);
                }

                db.Entry(admin).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ManageCashier");
            }

            return View(admin);
        }

        // Menampilkan konfirmasi hapus admin/cashier
        public ActionResult DeleteAdmin(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Admin admin = db.Admins.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }

            return View(admin);
        }

        // Menghapus data admin/cashier
        [HttpPost, ActionName("DeleteAdmin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAdminConfirmed(string id)
        {
            Admin admin = db.Admins.Find(id);
            db.Admins.Remove(admin);
            db.SaveChanges();
            return RedirectToAction("ManageCashier");
        }
    }
}