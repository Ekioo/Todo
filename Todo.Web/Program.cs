using Todo.Core.Services;
using Todo.Web.Api;
using Todo.Web.Components;

var builder = WebApplication.CreateBuilder(args);

var dataDir = Path.Combine(builder.Environment.ContentRootPath, "data");
builder.Services.AddSingleton(new ProjectService(dataDir));
builder.Services.AddSingleton<TicketService>();

builder.Services.AddOpenApi();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapOpenApi();
app.MapTodoApi();

app.MapGet("/api/docs", () =>
{
    var mdPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "API.md");
    if (!File.Exists(mdPath)) return Results.NotFound("API.md not found");
    return Results.Text(File.ReadAllText(mdPath), "text/markdown; charset=utf-8");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
