namespace ServicioUsuario.Infrastructure.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public bool UseDevelopmentMode { get; set; }
    public string Provider { get; set; } = "Smtp";
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Biblioteca";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public SmtpSettings Smtp { get; set; } = new();
}

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
