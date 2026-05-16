using ServicioUsuario.Domain.Common;

namespace ServicioUsuario.Domain.Errors;

public static class UsuarioErrors
{
    public static readonly Error CredencialesInvalidas = new("Usuario.Login", "Usuario o contraseña invalidos.");
    public static readonly Error RolInvalido = new("Usuario.Rol", "El rol seleccionado no es valido.");
    public static readonly Error DatosObligatorios = new("Usuario.Datos", "Completa todos los campos obligatorios.");
    public static readonly Error EmailInvalido = new("Usuario.Email", "El correo electronico ingresado no es valido.");
    public static readonly Error EmailDuplicado = new("Usuario.Email", "Ya existe un usuario registrado con ese correo.");
    public static readonly Error CiDuplicado = new("Usuario.CI", "Ya existe un usuario registrado con ese CI.");
    public static readonly Error NombreUsuarioDuplicado = new("Usuario.NombreUsuario", "No se pudo generar un nombre de usuario unico.");
    public static readonly Error UsuarioNoEncontrado = new("Usuario.Id", "El usuario no existe.");
    public static readonly Error UsuarioYaInactivo = new("Usuario.Estado", "El usuario ya se encuentra inactivo.");
    public static readonly Error NoAutorizado = new("Usuario.Autorizacion", "No tienes permisos para realizar esta accion.");
}
