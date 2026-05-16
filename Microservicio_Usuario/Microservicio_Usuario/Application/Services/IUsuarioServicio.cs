using System.Collections.Generic;
using ServicioUsuario.Domain.Common;
using ServicioUsuario.Domain.Entities;
using ServicioUsuario.Application.Dtos;

namespace ServicioUsuario.Application.Interfaces;

public interface IUsuarioServicio
{
    IEnumerable<UsuarioDto> Select();
    Result<Usuario> Login(string nombreUsuario, string passwordPlano);
    Task<Result> CrearUsuarioAsync(UsuarioDto usuarioDto, int usuarioSesionId, CancellationToken cancellationToken = default);
    Result CrearLector(LectorDto dto, int usuarioSesionId);
    IEnumerable<LectorDto> ObtenerLectores();
    Result DarDeBaja(int usuarioId, int usuarioSesionId);
    string JoinCiComp(string ci, string complemento);
}
