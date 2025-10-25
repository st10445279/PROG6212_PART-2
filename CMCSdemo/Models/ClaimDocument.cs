namespace CMCSdemo.Models
{
    public class ClaimDocument
    {
        public int DocumentId { get; set; }
        public int ClaimId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; } = DateTime.Now;
    }
}