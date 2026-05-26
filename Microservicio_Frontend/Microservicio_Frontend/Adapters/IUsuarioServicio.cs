using Frontend.Dtos;
using Frontend.Helpers;
using Frontend.Dtos;

namespace Frontend.Adapters;

public interface IUsuarioServicio
{
    IEnumerable<UsuarioDto> Select();
    Result<UsuarioDto> Create(UsuarioDto dto);
    Result<UsuarioDto> Login(string nombreUsuario, string password);
    Task<Result> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva, string passwordConfirmacion, CancellationToken ct = default);
    Task<Result> CrearUsuarioAsync(UsuarioDto dto, int usuarioSesionId, CancellationToken ct = default);
    Result CrearLector(LectorDto dto, int usuarioSesionId);
    Result DarDeBaja(int usuarioId, int usuarioSesionId);
}
