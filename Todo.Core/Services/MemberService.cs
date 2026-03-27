using Microsoft.EntityFrameworkCore;
using Todo.Core.Data;
using Todo.Core.Models;

namespace Todo.Core.Services;

public class MemberService
{
    private readonly ProjectService _projectService;

    public MemberService(ProjectService projectService)
    {
        _projectService = projectService;
    }

    private static async Task EnsureMemberTableAsync(TodoDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS Members (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            )
        """);
    }

    public async Task<List<Member>> ListMembersAsync(string projectSlug)
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        await EnsureMemberTableAsync(db);
        return await db.Members.OrderBy(m => m.Name).ToListAsync();
    }

    public async Task<Member> CreateMemberAsync(string projectSlug, string name)
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        await EnsureMemberTableAsync(db);
        var member = new Member { Name = name };
        db.Members.Add(member);
        await db.SaveChangesAsync();
        return member;
    }

    public async Task<Member?> UpdateMemberAsync(string projectSlug, int memberId, string name)
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        await EnsureMemberTableAsync(db);
        var member = await db.Members.FindAsync(memberId);
        if (member is null) return null;
        member.Name = name;
        await db.SaveChangesAsync();
        return member;
    }

    public async Task<bool> DeleteMemberAsync(string projectSlug, int memberId)
    {
        await using var db = _projectService.GetProjectDb(projectSlug);
        await EnsureMemberTableAsync(db);
        var member = await db.Members.FindAsync(memberId);
        if (member is null) return false;
        db.Members.Remove(member);
        await db.SaveChangesAsync();
        return true;
    }
}
