using System.Text.Json.Nodes;

namespace Todo.Core.Automation.Triggers;

/// <summary>
/// Fires when a new comment is added to any ticket, optionally filtered by author.
/// Persists the last-seen comment ID per ticket in dispatch-state.json under
/// "_lastCommentIds".
/// </summary>
public sealed class TicketCommentAddedTrigger : ITrigger
{
    private DateTime _lastPolled = DateTime.MinValue;
    private readonly TicketCommentAddedTriggerSpec _spec;

    public TicketCommentAddedTrigger(TicketCommentAddedTriggerSpec spec) { _spec = spec; }

    public async Task<IReadOnlyList<TriggerFiring>> EvaluateAsync(TriggerContext ctx, CancellationToken ct)
    {
        if ((ctx.Now - _lastPolled).TotalSeconds < _spec.PollSeconds)
            return Array.Empty<TriggerFiring>();
        _lastPolled = ctx.Now;

        var lastSeen = LoadLastCommentIds(ctx);
        var tickets = await ctx.Tickets.ListTicketsAsync(ctx.ProjectSlug);
        var firings = new List<TriggerFiring>();

        // Only inspect tickets that have comments (CommentCount > 0)
        foreach (var summary in tickets)
        {
            if (summary.CommentCount == 0) continue;

            var ticket = await ctx.Tickets.GetTicketAsync(ctx.ProjectSlug, summary.Id);
            if (ticket is null) continue;

            lastSeen.TryGetValue(summary.Id, out var prevMaxId);

            foreach (var comment in ticket.Comments)
            {
                if (comment.Id <= prevMaxId) continue;
                if (_spec.Authors.Count > 0 && !_spec.Authors.Contains(comment.Author, StringComparer.OrdinalIgnoreCase))
                    continue;
                firings.Add(new TriggerFiring(ticket.Id, ticket.Title, ticket.Status));
                break; // one firing per ticket per poll
            }

            var maxId = ticket.Comments.Count > 0 ? ticket.Comments.Max(c => c.Id) : 0;
            if (maxId > prevMaxId)
                lastSeen[summary.Id] = maxId;
        }

        SaveLastCommentIds(ctx, lastSeen);
        return firings;
    }

    private static Dictionary<int, int> LoadLastCommentIds(TriggerContext ctx)
    {
        var state = ctx.Sessions.Load(ctx.WorkspacePath);
        var node = state["_lastCommentIds"] as JsonObject;
        var dict = new Dictionary<int, int>();
        if (node is null) return dict;
        foreach (var kv in node)
            if (int.TryParse(kv.Key, out var ticketId) && kv.Value is not null)
                dict[ticketId] = kv.Value.GetValue<int>();
        return dict;
    }

    private static void SaveLastCommentIds(TriggerContext ctx, Dictionary<int, int> ids)
    {
        var state = ctx.Sessions.Load(ctx.WorkspacePath);
        var obj = new JsonObject();
        foreach (var kv in ids) obj[kv.Key.ToString()] = kv.Value;
        state["_lastCommentIds"] = obj;
        ctx.Sessions.Save(ctx.WorkspacePath, state);
    }
}
