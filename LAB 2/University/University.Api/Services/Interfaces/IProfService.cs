using University.Domain.Entities;

namespace University.Api.Services.Interfaces;

public interface IProfService
{
    Task<Prof?> GetByIdAsync(Guid id);
    Task<Prof> CreateAsync(Prof prof);
    Task<Prof?> UpdateAsync(Guid id, Prof updated);
    Task<bool> DeleteAsync(Guid id);
}