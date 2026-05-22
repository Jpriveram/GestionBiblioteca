using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ServicioUsuario.Application.Interfaces;
using ServicioUsuario.Application.Services;
using ServicioUsuario.Domain.Ports;
using ServicioUsuario.Infrastructure.Authentication; 
using ServicioUsuario.Infrastructure.Configuration;
using ServicioUsuario.Infrastructure.Email;
using ServicioUsuario.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Local overrides for secrets or machine-specific settings.
builder.Configuration.AddJsonFile("emailsettings.json", optional: true, reloadOnChange: false);

ConfigurationSingleton.Initialize(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

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
builder.Services.AddSingleton<UsuarioRepository>();

// Email sender (via factory: SMTP, Dev, or HTTP based on config)
builder.Services.AddSingleton<IEmailSender>(sp => EmailSenderFactory.Create(sp));

// Servicios
builder.Services.AddSingleton<IUserCredentialProvisioningService, UserCredentialProvisioningService>();
builder.Services.AddSingleton<IUsuarioServicio, UsuarioService>();

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