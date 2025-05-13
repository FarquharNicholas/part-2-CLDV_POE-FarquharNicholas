namespace ST10439055_POE.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        public DateTime ErrorTime { get; set; } = DateTime.Now;
        public string? Source { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool ShowMessage => !string.IsNullOrEmpty(Message);
        public bool ShowStackTrace => !string.IsNullOrEmpty(StackTrace);
        public bool ShowSource => !string.IsNullOrEmpty(Source);
    }
}