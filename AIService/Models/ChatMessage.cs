using System.ComponentModel.DataAnnotations;

namespace AIService.Models
{
    public enum MessageRole
    {
        User,
        Assistant
    }

    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }

        public int SessionId { get; set; }

        public MessageRole Role { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ChatSession Session { get; set; } = null!;
    }
}
