using University.Api.Services.Interfaces;
using University.Domain.Entities;
using University.Persistance;
using ZiggyCreatures.Caching.Fusion;

namespace University.Api.Services;

public class StudentService : IStudentService
{
	private readonly AppDbContext _context;
	private readonly IFusionCache _cache;

	public StudentService(AppDbContext context, IFusionCache cache)
	{
		_context = context;
		_cache = cache;
	}

	private static string CacheKey(Guid id) => $"student:{id}";

	public async Task<Student?> GetByIdAsync(Guid id)
	{
		return await _cache.GetOrSetAsync(
			CacheKey(id),
			async _ => await _context.Students.FindAsync(id),
			TimeSpan.FromMinutes(5)
		);
	}

	public async Task<Student> CreateAsync(Student student)
	{
		_context.Students.Add(student);
		await _context.SaveChangesAsync();

		await _cache.SetAsync(CacheKey(student.Id), student);

		return student;
	}

	public async Task<Student?> UpdateAsync(Guid id, Student updated)
	{
		var student = await _context.Students.FindAsync(id);
		if (student == null)
			return null;

		student.FirstName = updated.FirstName;
		student.LastName = updated.LastName;
		student.GroupId = updated.GroupId;

		await _context.SaveChangesAsync();

		await _cache.SetAsync(CacheKey(id), student);

		return student;
	}

	public async Task<bool> DeleteAsync(Guid id)
	{
		var student = await _context.Students.FindAsync(id);
		if (student == null)
			return false;

		_context.Students.Remove(student);
		await _context.SaveChangesAsync();

		await _cache.RemoveAsync(CacheKey(id));

		return true;
	}
}