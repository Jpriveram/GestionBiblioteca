var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient("ServicioPrestamo", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:ServicioPrestamo"] ?? "http://localhost:5103/"));
builder.Services.AddHttpClient("ServicioUsuario", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:ServicioUsuario"] ?? "http://localhost:5292/"));
builder.Services.AddHttpClient("ServicioLibroEjemplar", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:ServicioLibroEjemplar"] ?? "http://localhost:5101/"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
