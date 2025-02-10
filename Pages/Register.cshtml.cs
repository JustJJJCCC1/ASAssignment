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
using System.Text;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using ASAssignment1.Services;

namespace ASAssignment1.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _environment; // Inject environment for file saving
        private readonly IConfiguration _configuration; // Inject configuration

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(AuthDbContext context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Retrieve reCAPTCHA token from form
                string googleRecaptchaToken = Request.Form["g-recaptcha-response"].ToString();
                string secretKey = _configuration["ReCaptcha:SecretKey"];
                string verificationUrl = _configuration["ReCaptcha:VerificationUrl"];

                bool isValid = await RecaptchaService.verifyReCaptchaV3(googleRecaptchaToken, secretKey, verificationUrl);
                if (!isValid)
                {
                    ModelState.AddModelError(string.Empty, "Invalid reCAPTCHA. Please try again.");
                    return Page();
                }

                string relativeFilePath = null;
                if (RModel.ResumeFile != null)
                {
                    var fileExtension = Path.GetExtension(RModel.ResumeFile.FileName).ToLower();
                    if (fileExtension != ".pdf" && fileExtension != ".docx")
                    {
                        ModelState.AddModelError("RModel.ResumeFile", "Only PDF and DOCX files are allowed.");
                        return Page();
                    }

                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
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
                    NRIC = EncryptNRIC(RModel.NRIC),
                    DateOfBirth = RModel.DateOfBirth,
                    ResumeFilePath = relativeFilePath, // Store only the relative path
                    WhoAmI = EncodeWhoAmI(RModel.WhoAmI)
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

        // Encrypt the NRIC using AES
        private string EncryptNRIC(string nric)
        {
            using (Aes aesAlg = Aes.Create())
            {
                // Retrieve the AES Key and IV directly from appsettings.json
                string keyBase64 = _configuration["EncryptionKeys:AESKey"];
                string ivBase64 = _configuration["EncryptionKeys:AESIV"];

                byte[] key = Convert.FromBase64String(keyBase64);
                byte[] iv = Convert.FromBase64String(ivBase64);

                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(nric);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray()); // Return encrypted NRIC as Base64 string
                }
            }
        }

        // Encode the WhoAmI string (URL encoding)
        private string EncodeWhoAmI(string whoAmI)
        {
            return Uri.EscapeDataString(whoAmI); // URL encode the WhoAmI string
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
