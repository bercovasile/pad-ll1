namespace University.Domain.Entities;

public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public Guid ProfId { get; set; }
}