using Todo.Core.Models;

namespace Todo.Web.Api;

public record CreateProjectRequest(string Name);
public record CreateTicketRequest(string Title, string Description = "", string CreatedBy = "owner", TicketStatus Status = TicketStatus.Backlog);
public record MoveTicketRequest(TicketStatus Status);
public record AddCommentRequest(string Content, string Author = "owner");
