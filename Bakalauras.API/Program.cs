using Bakalauras.Persistence;
using Bakalauras.Persistence.Repositories;
using Bakalauras.Persistence.Repositories.EF;
using Bakalauras.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();  // Ensure logging is added

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), options =>
    {
        options.MigrationsAssembly("Bakalauras.Persistence");
    });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Bakalauras.API", Version = "v1" });
});

// Register repositories and services
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<INodeConnectionRepository, NodeConnectionRepository>();
builder.Services.AddScoped<NodeService>();
builder.Services.AddScoped<NodeConnectionService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bakalauras.API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
