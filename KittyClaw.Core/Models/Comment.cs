namespace KittyClaw.Core.Models;

public class Comment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public required string Content { get; set; }
    public string Author { get; set; } = "owner";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [System.Text.Json.Serialization.JsonIgnore]
    public Ticket Ticket { get; set; } = null!;
}
