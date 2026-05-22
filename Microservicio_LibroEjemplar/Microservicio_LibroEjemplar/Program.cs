using Scalar.AspNetCore;
using ServicioLibroEjemplar.Application.Interfaces;
using ServicioLibroEjemplar.Application.Services;
using ServicioLibroEjemplar.Infrastructure.Configuration;
using ServicioLibroEjemplar.Infrastructure.Creators;
using ServicioLibroEjemplar.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

ConfigurationSingleton.Initialize(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Creadores concretos
builder.Services.AddSingleton<LibroRepositoryCreator>();
builder.Services.AddSingleton<EjemplarRepositoryCreator>();

// Repositorios
builder.Services.AddSingleton(sp => sp.GetRequiredService<LibroRepositoryCreator>().CreateRepository());
builder.Services.AddSingleton(sp => sp.GetRequiredService<EjemplarRepositoryCreator>().CreateRepository());

// Servicios
builder.Services.AddSingleton<ILibroService, LibroService>();
builder.Services.AddSingleton<IEjemplarService, EjemplarService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.MapControllers();

app.Run();
