using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerifyEmailForgotPassword.Data;
using VerifyEmailForgotPassword.Models;
using VerifyEmailForgotPassword.Services;

namespace VerifyEmailForgotPassword.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly AppDbContext _context;
    private readonly ICryptoService _cryptoService;
    public UserController(AppDbContext context, ICryptoService cryptoService)
    {
        _context = context;
        _cryptoService = cryptoService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "Email already exists" });
        }

        var passwordSalt = _cryptoService.GenerateSalt();
        var user = new User
        {
            Email = request.Email,
            PasswordSalt = passwordSalt,
            PasswordHash = _cryptoService.HashPassword(request.Password, passwordSalt),
            VerificationToken = CreateRandomToken(),
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Registration successful" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return BadRequest(new { message = "User not found" });
        }

        if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return BadRequest(new { message = "Password is incorrect" });
        }

        if (user.VerifiedAt == default)
        {
            return BadRequest(new { message = "Email is not verified" });
        }

        return Ok(new { message = "Login successful" });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.VerificationToken == token);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid token" });
        }

        user.VerifiedAt = DateTime.UtcNow;
        user.VerificationToken = null;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Email verified" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(UserForgotPasswordRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return BadRequest(new { message = "User not found" });
        }

        user.PasswordResetToken = CreateRandomToken();
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Reset token sent" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(UserResetPasswordRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.PasswordResetToken == request.Token);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid token" });
        }

        if (user.ResetTokenExpires < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Token has expired" });
        }

        var passwordSalt = _cryptoService.GenerateSalt();
        user.PasswordSalt = passwordSalt;
        user.PasswordHash = _cryptoService.HashPassword(request.Password, passwordSalt);
        user.PasswordResetToken = null;
        user.ResetTokenExpires = default;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Password reset successful" });
    }

    private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        var hash = _cryptoService.HashPassword(password, passwordSalt);
        return hash.SequenceEqual(passwordHash);
    }

    private string CreateRandomToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}