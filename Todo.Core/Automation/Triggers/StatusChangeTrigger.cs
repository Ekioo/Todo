namespace Todo.Core.Automation.Triggers;

/// <summary>
/// Fires when a ticket's status changes, optionally filtered by from/to columns.
/// Uses a persisted snapshot (dispatch-state.json:_ticketSnapshot) to detect changes
/// across restarts.
///
/// A ticket's snapshot is only advanced after the engine confirms dispatch via
/// <see cref="CommitFiring"/>. Firings skipped by transient gates (concurrency,
/// dedup, budget) leave the snapshot at its old value, so the next poll re-fires.
/// Concurrent re-fires during an in-flight run are harmless — the engine's dedup
/// gate absorbs them.
/// </summary>
public sealed class StatusChangeTrigger : ITrigger
{
    private DateTime _lastPolled = DateTime.MinValue;
    private readonly StatusChangeTriggerSpec _spec;

    public StatusChangeTrigger(StatusChangeTriggerSpec spec) { _spec = spec; }

    public async Task<IReadOnlyList<TriggerFiring>> EvaluateAsync(TriggerContext ctx, CancellationToken ct)
    {
        if ((ctx.Now - _lastPolled).TotalSeconds < _spec.PollSeconds)
            return Array.Empty<TriggerFiring>();
        _lastPolled = ctx.Now;

        var previous = ctx.Sessions.TicketSnapshot(ctx.WorkspacePath);
        var tickets = await ctx.Tickets.ListTicketsAsync(ctx.ProjectSlug);
        var current = tickets.ToDictionary(t => t.Id, t => t.Status);

        var firings = new List<TriggerFiring>();
        var newSnapshot = new Dictionary<int, string>(current.Count);
        foreach (var (id, status) in current)
        {
            previous.TryGetValue(id, out var prevStatus);
            var shouldFire = prevStatus != status
                && (_spec.From is null || prevStatus == _spec.From)
                && (_spec.To is null || status == _spec.To);

            if (shouldFire)
            {
                var ticket = tickets.First(t => t.Id == id);
                firings.Add(new TriggerFiring(id, ticket.Title, status));
                // Keep old snapshot value so the firing is retried if not committed.
                if (prevStatus is not null) newSnapshot[id] = prevStatus;
            }
            else
            {
                newSnapshot[id] = status;
            }
        }

        ctx.Sessions.SaveTicketSnapshot(ctx.WorkspacePath, newSnapshot);
        return firings;
    }

    public Task CommitFiringAsync(TriggerContext ctx, TriggerFiring firing)
    {
        if (firing.TicketId is int tid && firing.TicketStatus is { } status)
        {
            var snapshot = ctx.Sessions.TicketSnapshot(ctx.WorkspacePath);
            snapshot[tid] = status;
            ctx.Sessions.SaveTicketSnapshot(ctx.WorkspacePath, snapshot);
        }
        return Task.CompletedTask;
    }
}
