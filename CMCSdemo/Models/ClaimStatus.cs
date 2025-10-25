namespace CMCSdemo.Models
{
    public enum ClaimStatus
    {
        Pending,
        SubmittedToCoordinator,
        VerifiedByCoordinator,
        RejectedByCoordinator,
        SubmittedToManager,
        ApprovedByManager,
        RejectedByManager,
        Completed
    }
}