using InboxService.Entities;
using Microsoft.EntityFrameworkCore;

namespace InboxService.Data;

public class InboxDbContext(DbContextOptions<InboxDbContext> options) : DbContext(options)
{
    public DbSet<MessageEntity> Messages { get; set; }
}