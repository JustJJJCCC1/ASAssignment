using ASAssignment1.Model; // Ensure you are using your custom User model
using ASAssignment1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace ASAssignment1.Pages
{
	public class RegisterModel : PageModel
	{
		private readonly AuthDbContext _context; // Use your custom DbContext
		[BindProperty]
		public Register RModel { get; set; }

		public RegisterModel(AuthDbContext context)
		{
			_context = context;
		}

		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				// Create a new user using your custom model
				var user = new User
				{
					FirstName = RModel.FirstName,
					LastName = RModel.LastName,
					Email = RModel.Email,
					Gender = RModel.Gender,
					NRIC = RModel.NRIC,
					DateOfBirth = RModel.DateOfBirth,
					ResumeFilePath = RModel.ResumeFilePath,
					WhoAmI = RModel.WhoAmI
				};

				// Hash the password
				user.Password = HashPassword(RModel.Password);

				//Hash the NRIC
				user.NRIC = HashPassword(RModel.NRIC);

				// Check if the user already exists
				if (_context.Users.Any(u => u.Email == user.Email))
				{
					ModelState.AddModelError("Email", "Email already exists");
					return Page();
				}


				// Save the user to the database
				_context.Users.Add(user);
				await _context.SaveChangesAsync();

				// Optionally sign in the user here if needed, or redirect to a different page
				return RedirectToPage("Index");
			}
			return Page();
		}

		// Hash the password (you can use a stronger hash like bcrypt, but SHA256 is shown for simplicity)
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
