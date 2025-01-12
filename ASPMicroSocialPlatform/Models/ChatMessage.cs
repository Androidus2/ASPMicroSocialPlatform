namespace ASPMicroSocialPlatform.Models
{
    public class ChatMessage
    {
        public int? Id { get; set; }
        public string? UserId { get; set; }
        public string? UserProfilePicture { get; set; }
        public int? GroupId { get; set; }
        public string? UserName { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
