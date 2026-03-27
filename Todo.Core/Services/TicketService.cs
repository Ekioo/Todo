using Microsoft.EntityFrameworkCore;
using Todo.Core.Data;
using Todo.Core.Models;

namespace Todo.Core.Services;

public class TicketService
{
    private readonly ProjectService _projectService;

    public TicketService(ProjectService projectService)
    {
        _projectService = projectService;
    }

    public async Task<List<Ticket>> ListTicketsAsync(string projectSlug, TicketStatus? statusFilter = null)
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        var query = db.Tickets.AsQueryable();
        if (statusFilter.HasValue)
            query = query.Where(t => t.Status == statusFilter.Value);
        return await query.OrderBy(t => t.CreatedAt).ToListAsync();
    }

    public async Task<Ticket?> GetTicketAsync(string projectSlug, int ticketId)
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        return await db.Tickets.Include(t => t.Comments.OrderBy(c => c.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == ticketId);
    }

    public async Task<Ticket> CreateTicketAsync(string projectSlug, string title, string description = "", string createdBy = "owner", TicketStatus status = TicketStatus.Backlog)
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        var ticket = new Ticket
        {
            Title = title,
            Description = description,
            CreatedBy = createdBy,
            Status = status
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket?> MoveTicketAsync(string projectSlug, int ticketId, TicketStatus newStatus)
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        var ticket = await db.Tickets.FindAsync(ticketId);
        if (ticket is null) return null;
        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return ticket;
    }

    public async Task<bool> DeleteTicketAsync(string projectSlug, int ticketId)
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        var ticket = await db.Tickets.Include(t => t.Comments).FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket is null) return false;
        db.Comments.RemoveRange(ticket.Comments);
        db.Tickets.Remove(ticket);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Comment?> AddCommentAsync(string projectSlug, int ticketId, string content, string author = "owner")
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        var ticket = await db.Tickets.FindAsync(ticketId);
        if (ticket is null) return null;
        var comment = new Comment
        {
            TicketId = ticketId,
            Content = content,
            Author = author
        };
        db.Comments.Add(comment);
        ticket.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return comment;
    }
}
