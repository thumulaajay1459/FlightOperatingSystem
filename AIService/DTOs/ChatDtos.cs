namespace AIService.DTOs
{
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        // If null, a new session is created
        public int? SessionId { get; set; }
    }

    public class ChatResponse
    {
        public int SessionId { get; set; }
        public string Reply { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ChatSessionDto
    {
        public int SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int MessageCount { get; set; }
    }

    public class ChatHistoryDto
    {
        public int SessionId { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = [];
    }

    public class ChatMessageDto
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
