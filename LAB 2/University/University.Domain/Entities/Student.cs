namespace University.Domain.Entities;

public class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid GroupId { get; set; }
}
