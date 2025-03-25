using Bakalauras.Persistence;
using Bakalauras.Persistence.Repositories;
using Bakalauras.Persistence.Repositories.EF;
using Bakalauras.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddControllers();

// Configure database connection with fixed connection string
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), options =>
    {
        options.MigrationsAssembly("Bakalauras.Persistence");
    });
});

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Bakalauras.API", Version = "v1" });
});

// ? CORS Configuration (Fixed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("https://navigacija-bvcwc2agfshpghdj.westeurope-01.azurewebsites.net") // Production URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register repositories and services
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<INodeConnectionRepository, NodeConnectionRepository>();
builder.Services.AddScoped<IBaseNodeRepository, BaseNodeRepository>();
builder.Services.AddScoped<NodeService>();
builder.Services.AddScoped<INodeNameSevice, NodeNameSevice>();
builder.Services.AddScoped<NodeConnectionService>();
builder.Services.AddScoped<BaseNodeService>();
builder.Services.AddScoped<DijkstraService>();

var app = builder.Build();

// Apply Middleware in Correct Order
// app.UseHttpsRedirection();  // Removed to prevent conflicts with Azure
app.UseCors("CorsPolicy");      //  Applied CORS before Authorization
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bakalauras.API v1");
    c.RoutePrefix = string.Empty; // Keeps Swagger UI at root URL
});

app.MapControllers();
app.Run();
