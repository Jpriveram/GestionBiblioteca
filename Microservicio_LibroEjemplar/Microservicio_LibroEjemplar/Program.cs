using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ServicioLibroEjemplar.Application.Interfaces;
using ServicioLibroEjemplar.Application.Services;
using ServicioLibroEjemplar.Infrastructure.Configuration;
using ServicioLibroEjemplar.Infrastructure.Creators;
using ServicioLibroEjemplar.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

ConfigurationSingleton.Initialize(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
    };
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.MapControllers();

app.Run();
