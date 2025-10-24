namespace Interchée.Services.Email.Entity
{
    public sealed class FrontendLinks
    {
        public string BaseUrl { get; set; } = "https://localhost:3000";
        public string ResetPasswordPath { get; set; } = "/reset-password";
        public string ConfirmEmailPath { get; set; } = "/confirm-email";
    }
}
