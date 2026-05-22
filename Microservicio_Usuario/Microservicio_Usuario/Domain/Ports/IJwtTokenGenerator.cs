using ServicioUsuario.Domain.Entities; 

namespace ServicioUsuario.Domain.Ports;

public interface IJwtTokenGenerator
{
    string GenerateToken(Usuario usuario);
}