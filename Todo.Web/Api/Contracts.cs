using Todo.Core.Models;

namespace Todo.Web.Api;

public record CreateProjectRequest(string Name);
public record CreateTicketRequest(string Title, string Description = "", string CreatedBy = "owner", TicketStatus Status = TicketStatus.Backlog, List<int>? LabelIds = null);
public record UpdateTicketRequest(string? Title = null, string? Description = null, string Author = "owner");
public record MoveTicketRequest(TicketStatus Status);
public record AddCommentRequest(string Content, string Author = "owner");
public record CreateLabelRequest(string Name, string Color = "#6366f1");
public record SetTicketLabelsRequest(List<int> LabelIds);
