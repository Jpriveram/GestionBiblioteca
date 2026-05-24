using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Scalar.AspNetCore;
using Microservicio_Autor.Application.Interfaces;
using Microservicio_Autor.Application.Services;
using Microservicio_Autor.Infrastructure.Configuration;
using Microservicio_Autor.Infrastructure.Creators;

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
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<AutorRepositoryCreator>();

builder.Services.AddSingleton(sp => sp.GetRequiredService<AutorRepositoryCreator>().CreateRepository());

builder.Services.AddSingleton<IAutorService, AutorService>();

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