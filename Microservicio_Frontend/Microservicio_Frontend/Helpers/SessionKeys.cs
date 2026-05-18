namespace Frontend.Helpers;

public static class SessionKeys
{
    public const string UsuarioId = "UsuarioId";
    public const string NombreUsuario = "NombreUsuario";
    public const string Rol = "Rol";
    public const string DebeCambiarPassword = "DebeCambiarPassword";
}

public class RouteTokenService
{
    public string CrearToken(int id) => id.ToString();
    public bool TryObtenerId(string token, out int id) => int.TryParse(token, out id);
}
