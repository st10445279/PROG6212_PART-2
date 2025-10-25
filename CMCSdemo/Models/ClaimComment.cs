namespace CMCSdemo.Models
{
    public class ClaimComment
    {
        public int CommentId { get; set; }
        public int ClaimId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}