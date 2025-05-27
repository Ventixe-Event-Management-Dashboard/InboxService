namespace InboxService.Models;

public class MessageItem
{
    public Guid Id { get; set; }
    public string SenderId { get; set; } = null!;
    public string Sender { get; set; } = null!;
    public string Receiver { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
}
