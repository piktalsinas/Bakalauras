using Bakalauras.Persistence;
using Bakalauras.Persistence.Repositories;
using Bakalauras.Persistence.Repositories.EF;
using Bakalauras.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bakalauras.API.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Services.AddLogging();
builder.Services.AddControllers();

// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), options =>
    {
        options.MigrationsAssembly("Bakalauras.Persistence");
    });
});

// Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Bakalauras.API", Version = "v1" });
});

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://localhost:3000")
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

// Register WebhookController (not needed if using [ApiController])
builder.Services.AddScoped<WebhookController>();

var app = builder.Build();

app.UseCors("CorsPolicy");
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bakalauras.API v1");
    c.RoutePrefix = string.Empty;
});

// Map controllers, including WebhookController
app.MapControllers();

app.Run();
