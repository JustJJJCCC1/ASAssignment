using ASAssignment1.Model; // Ensure you are using your custom User model
using ASAssignment1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace ASAssignment1.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _environment; // Inject environment for file saving

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(AuthDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                string relativeFilePath = null;
                if (RModel.ResumeFile != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(RModel.ResumeFile.FileName)}";
                    var uploadsFolder = Path.Combine("uploads"); // Relative path
                    var absolutePath = Path.Combine(_environment.WebRootPath, uploadsFolder, fileName);

                    // Ensure uploads directory exists
                    Directory.CreateDirectory(Path.Combine(_environment.WebRootPath, "uploads"));

                    using var fileStream = new FileStream(absolutePath, FileMode.Create);
                    await RModel.ResumeFile.CopyToAsync(fileStream);

                    relativeFilePath = Path.Combine(uploadsFolder, fileName).Replace("\\", "/"); // Store as "uploads/filename.ext"
                }

                var user = new User
                {
                    FirstName = RModel.FirstName,
                    LastName = RModel.LastName,
                    Email = RModel.Email,
                    Gender = RModel.Gender,
                    NRIC = HashPassword(RModel.NRIC),
                    DateOfBirth = RModel.DateOfBirth,
                    ResumeFilePath = relativeFilePath, // Store only the relative path
                    WhoAmI = RModel.WhoAmI
                };

                user.Password = HashPassword(RModel.Password);

                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return Page();
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToPage("Index");
            }
            return Page();
        }


        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
