using Todo.Core.Models;

namespace Todo.Web.Api;

public record CreateProjectRequest(string Name);
public record CreateTicketRequest(string Title, string CreatedBy, string Status, string Description = "", List<int>? LabelIds = null, TicketPriority Priority = TicketPriority.NiceToHave, string? AssignedTo = null);
public record UpdateTicketRequest(string Author, string? Title = null, string? Description = null, TicketPriority? Priority = null, string? AssignedTo = null);
public record MoveTicketRequest(string Status);
public record AddCommentRequest(string Content, string Author);
public record UpdateCommentRequest(string Content, string Author);
public record CreateLabelRequest(string Name, string Color = "#6366f1");
public record UpdateLabelRequest(string? Name = null, string? Color = null);
public record SetTicketLabelsRequest(List<int> LabelIds);
public record ReorderTicketRequest(string Status, int Index);
public record CreateColumnRequest(string Name, string Color = "#5a6a80");
public record ReorderColumnRequest(int ColumnId, int Index);
public record CreateMemberRequest(string Name);
public record UpdateMemberRequest(string Name);
