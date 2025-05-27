namespace InboxService.Models;

public class SendMessageRequest
{
    public string ReceiverId { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
}
