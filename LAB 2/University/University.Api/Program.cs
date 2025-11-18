using University.Api.Services;
using University.Api.Services.Interfaces;
using University.Persistance;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add your services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IProfService, ProfService>();
builder.Services.AddScoped<IGroupService, GroupService>();

// Add EF Core DbContext
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger (Swashbuckle) setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Serve Swagger JSON
    app.UseSwagger();

    // Serve Swagger UI at root (http://localhost:<port>/)
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "University API V1");
        c.RoutePrefix = string.Empty; // optional: serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
