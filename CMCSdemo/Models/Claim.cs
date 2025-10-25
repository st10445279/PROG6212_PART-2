namespace CMCSdemo.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount => HoursWorked * HourlyRate;
        public string AdditionalNotes { get; set; } = string.Empty;
        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? VerifiedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? CoordinatorComments { get; set; }
        public string? ManagerComments { get; set; }
        public List<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
    }
}