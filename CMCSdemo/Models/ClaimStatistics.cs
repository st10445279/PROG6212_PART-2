namespace CMCSdemo.Models
{
    public class ClaimStatistics
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalAmountClaimed { get; set; }
        public decimal TotalAmountApproved { get; set; }
        public decimal TotalAmountRejected { get; set; }
        public decimal AverageClaimAmount { get; set; }
        public Dictionary<string, int> ClaimsByMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> AmountByMonth { get; set; } = new Dictionary<string, decimal>();
        public List<LecturerPerformance> TopLecturers { get; set; } = new List<LecturerPerformance>();
    }

    public class LecturerPerformance
    {
        public string LecturerName { get; set; } = string.Empty;
        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public int ApprovedClaims { get; set; }
        public decimal ApprovalRate { get; set; }
    }
}