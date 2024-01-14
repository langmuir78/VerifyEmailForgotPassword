namespace VerifyEmailForgotPassword.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = null!;
    public byte[] PasswordSalt { get; set; } = null!;
    public string? VerificationToken { get; set; }
    public DateTime VerifiedAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime ResetTokenExpires { get; set; }
}
