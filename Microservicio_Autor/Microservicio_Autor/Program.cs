using Scalar.AspNetCore;
using Microservicio_Autor.Application.Interfaces;
using Microservicio_Autor.Application.Services;
using Microservicio_Autor.Infrastructure.Configuration;
using Microservicio_Autor.Infrastructure.Creators;

var builder = WebApplication.CreateBuilder(args);

ConfigurationSingleton.Initialize(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<AutorRepositoryCreator>();

builder.Services.AddSingleton(sp => sp.GetRequiredService<AutorRepositoryCreator>().CreateRepository());

builder.Services.AddSingleton<IAutorService, AutorService>();

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