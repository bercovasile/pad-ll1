using Microsoft.EntityFrameworkCore;
using University.Api.Services.Interfaces;
using University.Domain.Entities;
using University.Persistance;
using ZiggyCreatures.Caching.Fusion;

namespace University.Api.Services;

public class GroupService : IGroupService
{
    private readonly AppDbContext _context;
	private readonly IFusionCache _cache;
    public GroupService(AppDbContext context, IFusionCache cache)
    {
        _context = context;
        _cache = cache;
    }
	private static string CacheKey(Guid id) => $"group:{id}";


    public async Task<Group?> GetByIdAsync(Guid id)
    {
        return await _cache.GetOrSetAsync(
            CacheKey(id),
            async _ => await _context.Groups.FirstOrDefaultAsync(g => g.Id == id),
            TimeSpan.FromMinutes(5)
        );
    }


	public async Task<Group> CreateAsync(Group group)
	{
		_context.Groups.Add(group);
		await _context.SaveChangesAsync();

		await _cache.SetAsync(CacheKey(group.Id), group);

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

		await _cache.SetAsync(CacheKey(id), group);

		return group;
	}

	public async Task<bool> DeleteAsync(Guid id)
	{
		var group = await _context.Groups.FindAsync(id);
		if (group == null)
			return false;

		_context.Groups.Remove(group);
		await _context.SaveChangesAsync();

		await _cache.RemoveAsync(CacheKey(id));

		return true;
	}
}