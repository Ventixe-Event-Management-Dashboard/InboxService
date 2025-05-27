namespace InboxService.Entities;

public class MessageEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SenderId { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
