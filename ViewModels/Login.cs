using System.ComponentModel.DataAnnotations;

namespace ASAssignment1.ViewModels
{
    public class Login
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
