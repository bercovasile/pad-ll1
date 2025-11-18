using University.Domain.Entities;

namespace University.Api.Services.Interfaces;

public interface IStudentService
{
    Task<Student?> GetByIdAsync(Guid id);
    Task<Student> CreateAsync(Student student);
    Task<Student?> UpdateAsync(Guid id, Student updated);
    Task<bool> DeleteAsync(Guid id);
}
