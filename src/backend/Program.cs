using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WindSurfApi.Services;
using WindSurfApi.Services.Interfaces;
using WindSurfApi.Middleware;
using WindSurfApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WindSurf API", Version = "v1" });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Register services
builder.Services.AddScoped<ICsvDataProvider, CsvDataProvider>();
builder.Services.AddScoped<IAgenceService, AgenceService>();
builder.Services.AddScoped<IMagasinService, MagasinService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IAdminService, AdminService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// API Routes
var api = app.MapGroup("/api");
var agences = api.MapGroup("/agences");
var admin = api.MapGroup("/admin");

// Agences routes
agences.MapGet("/", (IAgenceService agenceService) =>
    Results.Ok(agenceService.GetAgences()));

agences.MapGet("/{agence}/magasins", (string agence, IMagasinService magasinService) =>
    Results.Ok(magasinService.GetMagasins(agence)));

agences.MapGet("/{agence}/magasins/{magasin}/articles", (string agence, string magasin, IArticleService articleService) =>
    Results.Ok(articleService.GetArticles(agence, magasin)));

agences.MapPost("/{agence}/magasins/{magasin}/articles", async (string agence, string magasin, List<UpdateQuantiteRequest> articles, IArticleService articleService) =>
{
    await articleService.UpdateQuantites(agence, magasin, articles);
    return Results.Ok();
});

// Admin routes
admin.MapGet("/csv", async (IAdminService adminService) =>
    Results.File(await adminService.DownloadCsvFile(), "text/csv", "inventory.csv"));

admin.MapPost("/csv", async (IFormFile file, IAdminService adminService) =>
{
    await adminService.MergeCsvFile(file);
    return Results.Ok();
})
.DisableAntiforgery();

admin.MapDelete("/csv", async (IAdminService adminService) =>
{
    await adminService.DeleteCsvFile();
    return Results.Ok();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.Run();
