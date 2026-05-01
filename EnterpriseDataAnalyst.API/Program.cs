using EnterpriseDataAnalyst.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using EnterpriseDataAnalyst.Application.Interfaces;
using EnterpriseDataAnalyst.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AI Services
builder.Services.AddHttpClient<IAiService, AiService>();
builder.Services.AddHttpClient<IWebSearchAgent, WebSearchAgent>();
builder.Services.AddScoped<IRouterAgent, RouterAgent>();
builder.Services.AddScoped<IPlannerAgent, PlannerAgent>();
builder.Services.AddScoped<IRagAgent, RagAgent>();
builder.Services.AddScoped<IDataAgent, DataAgent>();
builder.Services.AddScoped<IAnalysisAgent, AnalysisAgent>();
builder.Services.AddScoped<IValidationAgent, ValidationAgent>();
builder.Services.AddScoped<IOrchestratorService, OrchestratorService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Execute Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var enableMigrationAndSeeding = configuration.GetValue<bool>("EnableAutoMigrationAndSeeding");

    if (enableMigrationAndSeeding)
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dbCreator = context.Database.GetService<IRelationalDatabaseCreator>();

        // Step 1: Ensure the database itself exists
        await dbCreator.EnsureCreatedAsync();

        // Step 2: Check if the schema (tables) exist; if not, create them directly from the models
        if (!await dbCreator.HasTablesAsync())
        {
            await dbCreator.CreateTablesAsync();
        }

        // Seed data into the tables
        await DatabaseSeeder.SeedAsync(context);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Minimal API Endpoints for testing seeded anomalies
app.MapGet("/api/sales", async (AppDbContext db) => await db.Sales.Take(100).ToListAsync()).WithOpenApi();
app.MapGet("/api/sales/filter", async (string? region, int? year, int? month, AppDbContext db) => await db.Sales.Where(s => (string.IsNullOrEmpty(region) || s.Region == region) && (!year.HasValue || s.Date.Year == year) && (!month.HasValue || s.Date.Month == month)).ToListAsync()).WithOpenApi();
app.MapGet("/api/sales/summary", async (string? region, int? year, int? month, AppDbContext db) => await db.Sales.Include(s=>s.Product).Where(s => (string.IsNullOrEmpty(region) || s.Region == region) && (!year.HasValue || s.Date.Year == year) && (!month.HasValue || s.Date.Month == month)).GroupBy(s => s.Product.Name).Select(g => new { Product = g.Key, TotalAmount = g.Sum(x => x.Amount) }).ToListAsync()).WithOpenApi();
app.MapGet("/api/products", async (AppDbContext db) => await db.Products.ToListAsync()).WithOpenApi();
app.MapGet("/api/customers", async (AppDbContext db) => await db.Customers.Take(50).ToListAsync()).WithOpenApi();

app.Run();
