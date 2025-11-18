using University.Domain.Entities;

namespace University.Api.Services.Interfaces;

public interface IGroupService
{
    Task<Group?> GetByIdAsync(Guid id);
    Task<Group> CreateAsync(Group group);
    Task<Group?> UpdateAsync(Guid id, Group updated);
    Task<bool> DeleteAsync(Guid id);
}