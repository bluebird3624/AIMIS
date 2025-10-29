namespace Interchée.Services.Email.Entity
{
    public sealed class EmailSettings
    {
        public string FromName { get; set; } = "InternAttache";
        public string FromAddress { get; set; } = default!;

        public SmtpSettings Smtp { get; set; } = new();
        public FrontendLinks Frontend { get; set; } = new();

    }
}
