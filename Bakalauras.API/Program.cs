using Bakalauras.Persistence;
using Bakalauras.Persistence.Repositories;
using Bakalauras.Persistence.Repositories.EF;
using Bakalauras.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bakalauras.API.Controllers;
using Bakalauras.Domain.Models;
using Bakalauras.App.Services.IServices;
//using Bakalauras.Persistence.Repositories.IServices;
//using static BaseNodeService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddControllers();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

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

builder.Services.AddScoped<INodeConnectionService, NodeConnectionService>();
builder.Services.AddScoped<IDijkstraService, DijkstraService>();
builder.Services.AddScoped<INodeRepository, NodeRepository>();
builder.Services.AddScoped<INodeConnectionRepository, NodeConnectionRepository>();
builder.Services.AddScoped<IBaseNodeRepository, BaseNodeRepository>();
builder.Services.AddScoped<NodeService>();
builder.Services.AddScoped<INodeNameSevice, NodeNameSevice>();
builder.Services.AddScoped<NodeConnectionService>();
//builder.Services.AddScoped<INodeConnectionService, NodeConnectionService>();
builder.Services.AddScoped<BaseNodeService>();
builder.Services.AddScoped<DijkstraService>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddScoped<FacebookPayloadHandler>();
builder.Services.AddScoped<FacebookMessageService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<WitAiService>();



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

app.MapControllers();

app.Run();
