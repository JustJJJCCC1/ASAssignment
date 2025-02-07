using ASAssignment1.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace ASAssignment1.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly AuthDbContext _context;

        public LogoutModel(AuthDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
            // Capture the logged-in user's email
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (userEmail != null)
            {
                // Log the logout activity in the audit log
                _context.AuditLogs.Add(new AuditLog
                {
                    Email = userEmail,
                    Activity = "User logged out",
                    Timestamp = DateTime.UtcNow
                });

                // Save changes to the audit log
                _context.SaveChanges();
            }

            // Clear the session
            HttpContext.Session.Clear();

            // Optionally, remove authentication cookies if using authentication
            Response.Cookies.Delete(".AspNetCore.Identity.Application");

            // Set a message for the user
            TempData["LogoutMessage"] = "You Have Logged Out Successfully";
        }
    }
}
