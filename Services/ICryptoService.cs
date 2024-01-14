namespace VerifyEmailForgotPassword.Services;

public interface ICryptoService
{
    byte[] HashPassword(string password, byte[] salt);
    byte[] GenerateSalt();
}