using Frontend.Adapters;
using Frontend.Helpers;

var builder = WebApplication.CreateBuilder(args);

// HttpClient para microservicios (acepta certificados de desarrollo)
// builder.Services.AddHttpClient("ServicioPrestamo", c =>
//     c.BaseAddress = new Uri(builder.Configuration["ApiUrls:ServicioPrestamo"] ?? "http://localhost:5103/"))
//     .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//     {
//         ServerCertificateCustomValidationCallback = (_, _, _, _) => true
//     });
builder.Services.AddHttpClient("ServicioUsuario", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiUrls:ServicioUsuario"] ?? "http://localhost:5292/"));

// Adapters (implementan interfaces que las páginas esperan)
builder.Services.AddScoped<IAutorServicio, AutorAdapter>();
builder.Services.AddScoped<ILibroServicio, LibroAdapter>();
builder.Services.AddScoped<IEjemplarServicio, EjemplarAdapter>();
// builder.Services.AddScoped<IPrestamoServicio, PrestamoServicioAdapter>();
// builder.Services.AddScoped<IDetalleServicio, DetalleServicioAdapter>();
builder.Services.AddScoped<IUsuarioServicio, UsuarioAdapter>();
// builder.Services.AddScoped<IPrestamoFachada, PrestamoFachadaHttpAdapter>();
// builder.Services.AddScoped<IAnulacionFachada, AnulacionFachadaAdapter>();
// builder.Services.AddScoped<IEjemplarDisponibilidadFachada, EjemplarDisponibilidadFachadaAdapter>();
builder.Services.AddScoped<RouteTokenService>();

// Sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages(o =>
{
    o.RootDirectory = "/";
    o.Conventions.AddPageRoute("/Pages/Index", "");
    o.Conventions.AddPageRoute("/Pages/Index", "Index");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseRouting();
app.UseSession();

app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.Run();