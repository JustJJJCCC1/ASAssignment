namespace ASAssignment1.Model
{
    public class PasswordHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Password { get; set; }
        public DateTime DateChanged { get; set; }

        public User User { get; set; }
    }
}
