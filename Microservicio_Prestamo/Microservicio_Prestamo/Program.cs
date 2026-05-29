using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Microservicio_Prestamo.Infrastructure.Configuration;
using Microservicio_Prestamo.Infrastructure.Creators;
using Microservicio_Prestamo.Infrastructure.Persistence;
using Microservicio_Prestamo.Infrastructure.Background;
using Microservicio_Prestamo.Domain.Ports;
using Microservicio_Prestamo.Application.Interfaces;
using Microservicio_Prestamo.Application.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

ConfigurationSingleton.Initialize(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// JWT
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

// Repositorio
builder.Services.AddSingleton<PrestamoRepositoryCreator>();
builder.Services.AddSingleton<IPrestamoRepository>(sp =>
    sp.GetRequiredService<PrestamoRepositoryCreator>().CreateRepository());
builder.Services.AddSingleton<IOutboxRepository, OutboxRepository>();

// Servicio
builder.Services.AddSingleton<IPrestamoService, PrestamoService>();

// Outbox processor
builder.Services.AddHostedService<OutboxProcessor>();

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
