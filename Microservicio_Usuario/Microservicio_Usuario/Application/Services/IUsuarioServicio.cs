using ServicioUsuario.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ServicioUsuario.Application.Interfaces;

public interface IUsuarioServicio
{
    Task<List<UsuarioDto>> GetAllAsync();
    Task<UsuarioDto?> GetByIdAsync(int id);
    Task<UsuarioDto?> GetByEmailAsync(string email);
    Task<UsuarioDto?> GetByCIAsync(string ci);
    Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto);
    Task<UsuarioDto?> UpdateAsync(int id, UpdateUsuarioDto dto);
    Task<bool> DeleteAsync(int id);
    Task<(UsuarioDto? Usuario, string? Token)> LoginAsync(string nombreUsuario, string password);
    Task VerificarPasswordActualAsync(int usuarioId, string passwordActual);
    Task CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva, string passwordConfirmacion);
}