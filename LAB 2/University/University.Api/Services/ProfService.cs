using Microsoft.EntityFrameworkCore;
using University.Api.Services.Interfaces;
using University.Domain.Entities;
using University.Persistance;

namespace University.Api.Services;

public class ProfService : IProfService
{
    private readonly AppDbContext _context;

    public ProfService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Prof?> GetByIdAsync(Guid id)
    {
        return await _context.Profs
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Prof> CreateAsync(Prof prof)
    {
        _context.Profs.Add(prof);
        await _context.SaveChangesAsync();
        return prof;
    }

    public async Task<Prof?> UpdateAsync(Guid id, Prof updated)
    {
        var prof = await _context.Profs.FindAsync(id);
        if (prof == null)
            return null;

        prof.FullName = updated.FullName;

        await _context.SaveChangesAsync();
        return prof;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var prof = await _context.Profs.FindAsync(id);
        if (prof == null)
            return false;

        _context.Profs.Remove(prof);
        await _context.SaveChangesAsync();
        return true;
    }
}