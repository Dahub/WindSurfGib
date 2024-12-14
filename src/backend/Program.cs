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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// API Routes
var api = app.MapGroup("/api");
var agences = api.MapGroup("/agences");

// GET /api/agences
agences.MapGet("/", (IAgenceService agenceService) =>
    Results.Ok(agenceService.GetAgences()));

// GET /api/agences/{agence}/magasins
agences.MapGet("/{agence}/magasins", (string agence, IMagasinService magasinService) =>
    Results.Ok(magasinService.GetMagasins(agence)));

// GET /api/agences/{agence}/magasins/{magasin}/articles
agences.MapGet("/{agence}/magasins/{magasin}/articles", (string agence, string magasin, IArticleService articleService) =>
    Results.Ok(articleService.GetArticles(agence, magasin)));

// POST /api/agences/{agence}/magasins/{magasin}/articles
agences.MapPost("/{agence}/magasins/{magasin}/articles", async (string agence, string magasin, List<UpdateQuantiteRequest> articles, IArticleService articleService) =>
{
    await articleService.UpdateQuantites(agence, magasin, articles);
    return Results.Ok();
});

app.Run();
