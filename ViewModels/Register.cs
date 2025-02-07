using System.ComponentModel.DataAnnotations;

namespace ASAssignment1.ViewModels
{
    public class Register
    {
        [Required]
        [MaxLength(50)]
		[RegularExpression(@"^[A-Za-z]+$")]
		public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
		[RegularExpression(@"^[A-Za-z]+$")]
		public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [MaxLength(9)]
		[RegularExpression(@"^[A-Za-z]\d{7}[A-Za-z]$")]
		public string NRIC { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")]
		public string Password { get; set; }

        [Required]
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")]
		[Compare(nameof(Password), ErrorMessage = "Password and confirmation does not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string ResumeFilePath { get; set; } // Store file path or filename

        [Required]
        [MaxLength(1000)] // Optional length limit
        public string WhoAmI { get; set; }
    }
}
