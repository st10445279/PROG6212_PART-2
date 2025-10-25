namespace CMCSdemo.Models
{
    public class LecturerProfile
    {
        public int ProfileId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public decimal DefaultHourlyRate { get; set; }
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; } = DateTime.Now;
    }
}