using System.ComponentModel.DataAnnotations;

namespace AIService.Models
{
    public class ChatSession
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public ICollection<ChatMessage> Messages { get; set; } = [];
    }
}
