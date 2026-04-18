using System.Text.Json.Nodes;
using Todo.Core.Models;

namespace Todo.Core.Automation.Triggers;

/// <summary>
/// Fires on a parent ticket when the CSV of its sub-ticket statuses changes
/// compared to the previously recorded one. Reproduces Lain's
/// `producer.lastSubStatuses` gating to avoid re-dispatching the producer while
/// nothing actionable has changed in the sub-tree.
///
/// The CSV marker is only persisted after the engine confirms the dispatch via
/// <see cref="CommitFiringAsync"/> — firings skipped by transient gates are retried.
/// </summary>
public sealed class SubTicketStatusTrigger : ITrigger
{
    private DateTime _lastPolled = DateTime.MinValue;
    private readonly SubTicketStatusTriggerSpec _spec;

    public SubTicketStatusTrigger(SubTicketStatusTriggerSpec spec) { _spec = spec; }

    public async Task<IReadOnlyList<TriggerFiring>> EvaluateAsync(TriggerContext ctx, CancellationToken ct)
    {
        if ((ctx.Now - _lastPolled).TotalSeconds < _spec.PollSeconds)
            return Array.Empty<TriggerFiring>();
        _lastPolled = ctx.Now;

        var parents = await ctx.Tickets.ListTicketsAsync(ctx.ProjectSlug,
            statusFilter: _spec.ParentColumn);

        var state = ctx.Sessions.Load(ctx.WorkspacePath);
        var agentKey = ctx.Automation.Id;
        var bucket = state[agentKey] as JsonObject ?? new JsonObject();
        var lastSubs = bucket["lastSubStatuses"] as JsonObject ?? new JsonObject();

        var firings = new List<TriggerFiring>();
        foreach (var parent in parents)
        {
            if (parent.SubTickets.Count == 0) continue;

            var csv = ComputeCsv(parent.SubTickets);
            var prev = lastSubs[parent.Id.ToString()]?.GetValue<string>();
            if (prev == csv) continue;

            if (_spec.DebounceSeconds is not null)
            {
                var lastAt = ctx.Sessions.LastDispatched(ctx.WorkspacePath, agentKey);
                if (lastAt is not null && (ctx.Now - lastAt.Value).TotalSeconds < _spec.DebounceSeconds.Value)
                    continue;
            }

            firings.Add(new TriggerFiring(parent.Id, parent.Title, parent.Status));
        }

        return firings;
    }

    public async Task CommitFiringAsync(TriggerContext ctx, TriggerFiring firing)
    {
        if (firing.TicketId is not int tid) return;
        var ticket = await ctx.Tickets.GetTicketAsync(ctx.ProjectSlug, tid);
        if (ticket is null) return;
        var csv = ComputeCsv(ticket.SubTickets);

        var state = ctx.Sessions.Load(ctx.WorkspacePath);
        var agentKey = ctx.Automation.Id;
        var bucket = state[agentKey] as JsonObject;
        if (bucket is null) { bucket = new JsonObject(); state[agentKey] = bucket; }
        var lastSubs = bucket["lastSubStatuses"] as JsonObject;
        if (lastSubs is null) { lastSubs = new JsonObject(); bucket["lastSubStatuses"] = lastSubs; }
        lastSubs[tid.ToString()] = csv;
        ctx.Sessions.Save(ctx.WorkspacePath, state);
    }

    private static string ComputeCsv(IEnumerable<SubTicketInfo> subs) =>
        string.Join(",", subs.OrderBy(s => s.Id).Select(s => $"{s.Id}:{s.Status}"));
}
