using Microsoft.EntityFrameworkCore;
using University.Api.Services.Interfaces;
using University.Domain.Entities;
using University.Persistance;

namespace University.Api.Services;

public class GroupService : IGroupService
{
    private readonly AppDbContext _context;

    public GroupService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Group?> GetByIdAsync(Guid id)
    {
        return await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Group> CreateAsync(Group group)
    {
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<Group?> UpdateAsync(Guid id, Group updated)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null)
            return null;

        group.Name = updated.Name;
        group.ProfId = updated.ProfId;

        await _context.SaveChangesAsync();
        return group;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null)
            return false;

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }
}