using Microsoft.OpenApi.Models;
using TeleCasino.BaccaratGameService.Services;
using TeleCasino.BaccaratGameService.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Enable controllers
builder.Services.AddControllers();

// Register BaccaratGameService as singleton
builder.Services.AddSingleton<IBaccaratGameService, BaccaratGameService>();

// Enable OpenAPI (Swagger UI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TeleCasino BaccaratGame API",
        Version = "v1",
        Description = "API to generate Baccarat game results and files."
    });
});

var app = builder.Build();

// Enable Swagger UI in dev environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TeleCasino BaccaratGame API v1");
        options.RoutePrefix = "swagger";
    });
}

// app.UseHttpsRedirection();

// Allow serving static files (JSON, MP4 results later)
app.UseStaticFiles();

app.MapControllers();

app.Run();
