using System.ComponentModel.DataAnnotations;

namespace ASAssignment1.Model
{
	public class User
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Password { get; set; }

		public string Gender { get; set; }

		public DateTime DateOfBirth { get; set; }

		public string NRIC { get; set; }

		public string ResumeFilePath { get; set; }

		public string WhoAmI { get; set; }
	}
}
