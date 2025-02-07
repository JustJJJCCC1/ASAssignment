using ASAssignment1.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using ASAssignment1.ViewModels;
using System;

namespace ASAssignment1.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AuthDbContext _context;
        private const int MaxFailedAttempts = 3;

        [BindProperty]
        public Login RModel { get; set; }

        public LoginModel(AuthDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = _context.Users.SingleOrDefault(u => u.Email == RModel.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }

            // Check if the account is locked
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                ModelState.AddModelError(string.Empty, "Account locked due to multiple failed login attempts.");
                return Page();
            }

            // Validate password
            if (user.Password != HashPassword(RModel.Password))
            {
                user.FailedLoginAttempts++;
                _context.SaveChanges();

                // Audit Log for failed login
                _context.AuditLogs.Add(new AuditLog
                {
                    Email = RModel.Email,
                    Activity = "Failed login attempt",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return base.Page();
            }

            // Reset failed attempts on successful login
            user.FailedLoginAttempts = 0;
            _context.SaveChanges();

            // Store session
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserId", user.Id.ToString());

            // Audit Log for successful login
            _context.AuditLogs.Add(new AuditLog
            {
                Email = user.Email,
                Activity = "Successful login",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
