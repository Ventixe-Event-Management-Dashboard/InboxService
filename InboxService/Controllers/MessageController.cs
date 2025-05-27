using InboxService.Data;
using InboxService.Entities;
using InboxService.Models;
using InboxService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InboxService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/message")]
    public class MessageController(InboxDbContext context) : ControllerBase
    {
        private readonly InboxDbContext _context = context;

        private string? GetUserId() =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private string? GetUserEmail() =>
            User.FindFirst(ClaimTypes.Email)?.Value;

        [HttpGet]
        public async Task<IActionResult> GetUserMessages()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var messages = await _context.Messages
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessageById(Guid id, [FromServices] UserProfileServiceClient userClient)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();

            if (message.SenderId != userId && message.ReceiverId != userId)
                return Forbid();

            var users = await userClient.GetAllUsersAsync();

            var result = new MessageItem
            {
                Id = message.Id,
                SenderId = message.SenderId,
                Sender = users.FirstOrDefault(u => u.Id == message.SenderId)?.FullName ?? "Okänd",
                Receiver = users.FirstOrDefault(u => u.Id == message.ReceiverId)?.FullName ?? "Okänd",
                Subject = message.Subject,
                Content = message.Content,
                SentAt = message.SentAt
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var senderId = GetUserId();
            if (senderId == null)
                return Unauthorized();

            var message = new MessageEntity
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Subject = request.Subject,
                Content = request.Content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserMessages), new { id = message.Id }, message);
        }

        [HttpGet("email")]
        public IActionResult GetCurrentUserEmail()
        {
            var email = GetUserEmail();
            if (email == null)
                return Unauthorized();

            return Ok(new { email });
        }

        [HttpGet("recipients")]
        public async Task<IActionResult> GetAvailableRecipients([FromServices] UserProfileServiceClient userClient)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null)
                return Unauthorized();

            var users = await userClient.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetInboxMessages([FromServices] UserProfileServiceClient userClient)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var messages = await _context.Messages
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            var users = await userClient.GetAllUsersAsync();

            var result = messages.Select(m => new MessageItem
            {
                Id = m.Id,
                SenderId = m.SenderId,
                Sender = users.FirstOrDefault(u => u.Id == m.SenderId)?.FullName ?? "Okänd",
                Subject = m.Subject,
                Content = m.Content,
                SentAt = m.SentAt
            }).ToList();

            return Ok(result);
        }

        [HttpGet("sent")]
        public async Task<IActionResult> GetSentMessages([FromServices] UserProfileServiceClient userClient)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var messages = await _context.Messages
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            var users = await userClient.GetAllUsersAsync();

            var result = messages.Select(m => new MessageItem
            {
                Id = m.Id,
                SenderId = m.SenderId,
                Sender = users.FirstOrDefault(u => u.Id == m.SenderId)?.FullName ?? "Okänd",
                Subject = m.Subject,
                Content = m.Content,
                SentAt = m.SentAt
            }).ToList();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
                return NotFound();

            if (message.SenderId != userId && message.ReceiverId != userId)
                return Forbid();

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
