using ServicioUsuario.Domain.Entities;

namespace ServicioUsuario.Application.Interfaces;

public interface IUserCredentialProvisioningService
{
    Task<CredentialProvisioningResult> PrepareAndNotifyAsync(Usuario usuario, CancellationToken cancellationToken = default);
}

public class CredentialProvisioningResult
{
    public string GeneratedUserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordAlgorithm { get; set; } = string.Empty;

    public bool EmailSent { get; set; }
    public string? EmailError { get; set; }
}
