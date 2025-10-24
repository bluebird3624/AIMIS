namespace Interchée.Services.Email.Entity
{
    public sealed class SmtpSettings
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;
        public string? User { get; set; }
        public string? Password { get; set; }
        public string ClientName { get; set; } = "InternAttache-Api";
        public bool DisableCertificateValidation { get; set; } = false; // dev only
    }
}
