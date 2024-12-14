using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WindSurfApi.Services;
using WindSurfApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "WindSurf API", 
        Version = "v1",
        Description = "API pour la gestion des équipements"
    });
});

// Register services
builder.Services.AddSingleton<ICsvDataProvider, CsvDataProvider>();
builder.Services.AddScoped<IAgenceService, AgenceService>();
builder.Services.AddScoped<IMagasinService, MagasinService>();
builder.Services.AddScoped<IArticleService, ArticleService>();

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WindSurf API V1");
        c.RoutePrefix = string.Empty; // Pour servir Swagger UI à la racine
    });
}

app.UseCors();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
