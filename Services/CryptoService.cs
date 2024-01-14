using System.Security.Cryptography;
using System.Text;

namespace VerifyEmailForgotPassword.Services;

public class CryptoService : ICryptoService
{
    public byte[] HashPassword(string password, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public byte[] GenerateSalt()
    {
        using var hmac = new HMACSHA512();
        return hmac.Key;
    }
}