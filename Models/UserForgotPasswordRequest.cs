using System.ComponentModel.DataAnnotations;

namespace VerifyEmailForgotPassword.Models;

public class UserForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}