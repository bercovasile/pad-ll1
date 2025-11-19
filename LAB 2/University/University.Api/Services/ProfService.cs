using Microsoft.EntityFrameworkCore;
using University.Api.Services.Interfaces;
using University.Domain.Entities;
using University.Persistance;
using ZiggyCreatures.Caching.Fusion;

namespace University.Api.Services;

public class ProfService : IProfService
{
	private readonly AppDbContext _context;
	private readonly IFusionCache _cache;

	public ProfService(AppDbContext context, IFusionCache cache)
	{
		_context = context;
		_cache = cache;
	}

	private static string CacheKey(Guid id) => $"prof:{id}";
	public async Task<Prof?> GetByIdAsync(Guid id)
	{
		return await _cache.GetOrSetAsync(
			CacheKey(id),
			async _ => await _context.Profs.FirstOrDefaultAsync(p => p.Id == id),
			TimeSpan.FromMinutes(5)
		);
	}

	public async Task<Prof> CreateAsync(Prof prof)
	{
		_context.Profs.Add(prof);
		await _context.SaveChangesAsync();

		await _cache.SetAsync(CacheKey(prof.Id), prof);

		return prof;
	}

	public async Task<Prof?> UpdateAsync(Guid id, Prof updated)
	{
		var prof = await _context.Profs.FindAsync(id);
		if (prof == null)
			return null;

		prof.FullName = updated.FullName;

		await _context.SaveChangesAsync();

		await _cache.SetAsync(CacheKey(id), prof);

		return prof;
	}

	public async Task<bool> DeleteAsync(Guid id)
	{
		var prof = await _context.Profs.FindAsync(id);
		if (prof == null)
			return false;

		_context.Profs.Remove(prof);
		await _context.SaveChangesAsync();

		await _cache.RemoveAsync(CacheKey(id));

		return true;
	}
}