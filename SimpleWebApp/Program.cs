using Microsoft.EntityFrameworkCore;
using SimpleWebApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();

    // Retry logic: try 15 times with a 2-second pause (30 seconds total)
    for (int i = 0; i < 15; i++)
    {
        try
        {
            logger.LogInformation("Attempting to connect to the database (Attempt {Attempt})...", i + 1);
            context.Database.EnsureCreated();
            
            // --- Add Seeding Logic ---
            if (!context.Employees.Any())
            {
                logger.LogInformation("Seeding database with sample employees...");
                context.Employees.AddRange(
                    new SimpleWebApp.Models.Employee { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Department = "HR" },
                    new SimpleWebApp.Models.Employee { FirstName = "Bob", LastName = "Jones", Email = "bob@example.com", Department = "IT" }
                );
                context.SaveChanges();
                logger.LogInformation("Seeding complete.");
            }
            // -------------------------

            logger.LogInformation("Database is ready!");
            break;
        }
        catch (Exception ex)
        {
            if (i == 14) // Last attempt failed
            {
                logger.LogError(ex, "Could not connect to the database after 15 attempts.");
            }
            else
            {
                logger.LogWarning("Database not ready yet, waiting 2 seconds...");
                Thread.Sleep(2000);
            }
        }
    }
}

app.Run();
