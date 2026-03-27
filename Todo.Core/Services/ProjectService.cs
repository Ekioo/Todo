using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Todo.Core.Data;
using Todo.Core.Models;

namespace Todo.Core.Services;

public partial class ProjectService
{
    private readonly string _dataDir;
    private readonly string _registryPath;

    public ProjectService(string dataDir)
    {
        _dataDir = dataDir;
        _registryPath = Path.Combine(dataDir, "registry.db");
        Directory.CreateDirectory(dataDir);
    }

    public async Task<List<Project>> ListProjectsAsync()
    {
        await using var db = new RegistryDbContext(_registryPath);
        await db.Database.EnsureCreatedAsync();
        return await db.Projects.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Project> CreateProjectAsync(string name)
    {
        var slug = SlugRegex().Replace(name.ToLowerInvariant(), "-").Trim('-');
        if (string.IsNullOrEmpty(slug)) slug = "project";

        await using var registry = new RegistryDbContext(_registryPath);
        await registry.Database.EnsureCreatedAsync();

        // Ensure unique slug
        var existing = await registry.Projects.AnyAsync(p => p.Slug == slug);
        if (existing)
        {
            var i = 2;
            while (await registry.Projects.AnyAsync(p => p.Slug == $"{slug}-{i}")) i++;
            slug = $"{slug}-{i}";
        }

        var project = new Project { Name = name, Slug = slug };
        registry.Projects.Add(project);
        await registry.SaveChangesAsync();

        // Create the project database
        var projectDbPath = GetProjectDbPath(slug);
        await using var projectDb = new TodoDbContext(projectDbPath);
        await projectDb.Database.EnsureCreatedAsync();

        return project;
    }

    public async Task<Project?> GetProjectAsync(string slug)
    {
        await using var db = new RegistryDbContext(_registryPath);
        await db.Database.EnsureCreatedAsync();
        return await db.Projects.FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public string GetProjectDbPath(string slug) => Path.Combine(_dataDir, "projects", $"{slug}.db");

    public TodoDbContext GetProjectDb(string slug)
    {
        var path = GetProjectDbPath(slug);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var db = new TodoDbContext(path);
        db.Database.EnsureCreated();
        return db;
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex SlugRegex();
}
