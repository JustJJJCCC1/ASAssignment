using ASAssignment1.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace ASAssignment1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration; // Inject configuration

        public User User { get; private set; }
        public string DecryptedNRIC { get; private set; } // Store decrypted NRIC

        public IndexModel(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Login");
            }

            User = _context.Users.SingleOrDefault(u => u.Email == email);
            if (User == null)
            {
                return RedirectToPage("/Login");
            }

            // Decrypt NRIC before displaying
            if (!string.IsNullOrEmpty(User.NRIC))
            {
                DecryptedNRIC = DecryptNRIC(User.NRIC);
            }

            return Page();
        }
        private string DecryptNRIC(string encryptedNric)
        {
            using (Aes aesAlg = Aes.Create())
            {
                // Retrieve the AES Key and IV from appsettings.json
                string keyBase64 = _configuration["EncryptionKeys:AESKey"];
                string ivBase64 = _configuration["EncryptionKeys:AESIV"];

                byte[] key = Convert.FromBase64String(keyBase64);
                byte[] iv = Convert.FromBase64String(ivBase64);

                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedNric)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd(); // Return decrypted NRIC
                        }
                    }
                }
            }
        }
    }
}
