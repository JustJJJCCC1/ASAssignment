using ASAssignment1.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace ASAssignment1.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly AuthDbContext _context;

        public ChangePasswordModel(AuthDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; }

        public class ChangePasswordInputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Login");
            }
            var user = _context.Users.Include(u => u.PasswordHistory)
                             .SingleOrDefault(u => u.Email == email);

            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            var lastPasswordChange = user.PasswordHistory.OrderByDescending(ph => ph.DateChanged)
                                                         .FirstOrDefault()?.DateChanged;

            int minPasswordAgeDays = 1; // Minimum password age

            if (lastPasswordChange.HasValue && DateTime.UtcNow < lastPasswordChange.Value.AddDays(minPasswordAgeDays))
            {
                TempData["PasswordAgeRestriction"] = $"You can only change your password once every {minPasswordAgeDays} day(s).";
            }
            

            return Page();
        }

        public IActionResult OnPost()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Login");
            }

            var user = _context.Users.Include(u => u.PasswordHistory)
                             .SingleOrDefault(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            // Verify old password
            if (!VerifyPassword(Input.OldPassword, user.Password))
            {
                ModelState.AddModelError(string.Empty, "Incorrect old password.");
                return Page();
            }

            // Get the most recent password change date
            var lastPasswordChange = user.PasswordHistory.OrderByDescending(ph => ph.DateChanged)
                                                         .FirstOrDefault()?.DateChanged;

            int minPasswordAgeDays = 1; // Minimum password age (e.g., 1 day)

            if (lastPasswordChange.HasValue && DateTime.UtcNow < lastPasswordChange.Value.AddDays(minPasswordAgeDays))
            {
                ModelState.AddModelError(string.Empty, $"You can only change your password once every {minPasswordAgeDays} day(s).");
                return Page();
            }

            // Check if the new password is in the password history
            if (user.PasswordHistory.Any(ph => ph.Password == HashPassword(Input.NewPassword)))
            {
                ModelState.AddModelError(string.Empty, "You cannot reuse any of your last 2 passwords.");
                return Page();
            }

            // Hash the new password and save it in the password history
            string newPasswordHash = HashPassword(Input.NewPassword);

            // Store the new password hash in history
            if (user.PasswordHistory.Count >= 2)
            {
                _context.PasswordHistories.Remove(user.PasswordHistory.First());
            }

            user.PasswordHistory.Add(new PasswordHistory
            {
                Password = newPasswordHash,
                DateChanged = DateTime.UtcNow
            });

            // Update the user's password
            user.Password = newPasswordHash;


            // Add audit log entry
            _context.AuditLogs.Add(new AuditLog
            {
                Email = email,
                Activity = "Password changed",
                Timestamp = DateTime.UtcNow
            });

            _context.SaveChanges();
            return RedirectToPage("/Index");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return HashPassword(enteredPassword) == storedHash;
        }
    }
}
