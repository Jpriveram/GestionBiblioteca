using MongoDB.Driver;
using Scalar.AspNetCore;
using ServicioMultas.Application.Interfaces;
using ServicioMultas.Application.Services;
using ServicioMultas.Domain.Ports;
using ServicioMultas.Infrastructure.Configuration;
using ServicioMultas.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbSettings.SectionName));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<IMultaRepository>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    var database = sp.GetRequiredService<IMongoDatabase>();
    return new MultaRepository(database, settings.CollectionName);
});

// Servicio de aplicación
builder.Services.AddSingleton<IMultaService, MultaService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => Results.Redirect("/scalar/v1"));
app.MapControllers();
app.Run();
