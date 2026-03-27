using Todo.Core.Models;
using Todo.Core.Services;

namespace Todo.Web.Api;

public static class Endpoints
{
    public static void MapTodoApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // Projects
        api.MapGet("/projects", async (ProjectService ps) =>
            Results.Ok(await ps.ListProjectsAsync()))
            .WithTags("Projects");

        api.MapPost("/projects", async (CreateProjectRequest req, ProjectService ps) =>
        {
            var project = await ps.CreateProjectAsync(req.Name);
            return Results.Created($"/api/projects/{project.Slug}", project);
        }).WithTags("Projects");

        api.MapGet("/projects/{slug}", async (string slug, ProjectService ps) =>
        {
            var project = await ps.GetProjectAsync(slug);
            return project is null ? Results.NotFound() : Results.Ok(project);
        }).WithTags("Projects");

        // Tickets
        api.MapGet("/projects/{slug}/tickets", async (string slug, TicketStatus? status, TicketService ts) =>
            Results.Ok(await ts.ListTicketsAsync(slug, status)))
            .WithTags("Tickets");

        api.MapPost("/projects/{slug}/tickets", async (string slug, CreateTicketRequest req, TicketService ts) =>
        {
            var ticket = await ts.CreateTicketAsync(slug, req.Title, req.Description, req.CreatedBy, req.Status);
            return Results.Created($"/api/projects/{slug}/tickets/{ticket.Id}", ticket);
        }).WithTags("Tickets");

        api.MapGet("/projects/{slug}/tickets/{id:int}", async (string slug, int id, TicketService ts) =>
        {
            var ticket = await ts.GetTicketAsync(slug, id);
            return ticket is null ? Results.NotFound() : Results.Ok(ticket);
        }).WithTags("Tickets");

        api.MapPatch("/projects/{slug}/tickets/{id:int}/status", async (string slug, int id, MoveTicketRequest req, TicketService ts) =>
        {
            var ticket = await ts.MoveTicketAsync(slug, id, req.Status);
            return ticket is null ? Results.NotFound() : Results.Ok(ticket);
        }).WithTags("Tickets");

        api.MapDelete("/projects/{slug}/tickets/{id:int}", async (string slug, int id, TicketService ts) =>
        {
            var deleted = await ts.DeleteTicketAsync(slug, id);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).WithTags("Tickets");

        // Comments
        api.MapPost("/projects/{slug}/tickets/{id:int}/comments", async (string slug, int id, AddCommentRequest req, TicketService ts) =>
        {
            var comment = await ts.AddCommentAsync(slug, id, req.Content, req.Author);
            return comment is null ? Results.NotFound() : Results.Created($"/api/projects/{slug}/tickets/{id}", comment);
        }).WithTags("Comments");
    }
}
