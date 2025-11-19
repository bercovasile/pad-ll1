using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace University.Persistance;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use your PostgreSQL connection string
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=university;Username=postgres;Password=postgres;TrustServerCertificate=True;");

        return new AppDbContext(optionsBuilder.Options);
    }
}