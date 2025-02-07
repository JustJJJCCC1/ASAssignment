namespace ASAssignment1.Model
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Activity { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
