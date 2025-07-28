using CyberCloudDriveAPI.DTOs.Auth;
using CyberCloudDriveAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CyberCloudDriveAPI.Services
{
    using CyberCloudDriveAPI.Data;
    using Microsoft.EntityFrameworkCore;
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;

        public AuthService(UserManager<User> userManager, IConfiguration config, AppDbContext db)
        {
            _userManager = userManager;
            _config = config;
            _db = db;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var user = new User { UserName = dto.Email, Email = dto.Email, Name = dto.Name, CreatedAt = DateTime.UtcNow };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            // Generate OTP and store in DB
            var otp = new Random().Next(100000, 999999).ToString();
            var otpEntity = new OTP { UserId = user.Id, Otp = otp, ExpiresAt = DateTime.UtcNow.AddMinutes(10), Used = false };
            _db.OTPs.Add(otpEntity);
            await _db.SaveChangesAsync();
            // Send OTP via email using MailKit
            var smtpServer = _config["SmtpSettings:Server"];
            var smtpPort = int.Parse(_config["SmtpSettings:Port"] ?? "587");
            var smtpUser = _config["SmtpSettings:Username"];
            var smtpPass = _config["SmtpSettings:Password"];
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress("CyberCloud Drive", smtpUser));
            message.To.Add(new MimeKit.MailboxAddress(user.Name, user.Email));
            message.Subject = "Your OTP Code";
            message.Body = new MimeKit.TextPart("plain") { Text = $"Your OTP code is: {otp}" };
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new Exception("Invalid credentials");
            // TODO: Log activity (login)
            return await GenerateAuthResponse(user);
        }

        public async Task<bool> VerifyOtpAsync(OtpDto dto)
        {
            // Fetch OTP from DB and verify
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return false;
            var otpEntity = await _db.OTPs.Where(o => o.UserId == user.Id && o.Otp == dto.Otp && !o.Used && o.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();
            if (otpEntity == null) return false;
            otpEntity.Used = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return false;
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var smtpServer = _config["SmtpSettings:Server"];
            var smtpPort = int.Parse(_config["SmtpSettings:Port"] ?? "587");
            var smtpUser = _config["SmtpSettings:Username"];
            var smtpPass = _config["SmtpSettings:Password"];
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress("CyberCloud Drive", smtpUser));
            message.To.Add(new MimeKit.MailboxAddress(user.Name, user.Email));
            message.Subject = "Password Reset";
            message.Body = new MimeKit.TextPart("plain") { Text = $"Your password reset token is: {token}" };
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            // Find user by email (token is sent to email)
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return false;
            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
            return result.Succeeded;
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            // Remove refresh token from DB (if using refresh tokens)
            var tokens = _db.OTPs.Where(o => o.UserId == userId && o.Used == false);
            foreach (var token in tokens)
            {
                token.Used = true;
            }
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<AuthResponseDto> RefreshAsync(RefreshDto dto)
        {
            // Validate refresh token from DB
            var otpEntity = await _db.OTPs.FirstOrDefaultAsync(o => o.Otp == dto.RefreshToken && !o.Used && o.ExpiresAt > DateTime.UtcNow);
            if (otpEntity == null) throw new Exception("Invalid or expired refresh token");
            var user = await _userManager.FindByIdAsync(otpEntity.UserId);
            if (user == null) throw new Exception("User not found");
            otpEntity.Used = true;
            await _db.SaveChangesAsync();
            return await GenerateAuthResponse(user);
        }

        private Task<AuthResponseDto> GenerateAuthResponse(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("name", user.Name)
            };
            var secret = _config["JWT:Secret"] ?? "supersecret";
            byte[] keyBytes;
            try
            {
                keyBytes = Convert.FromBase64String(secret);
            }
            catch
            {
                keyBytes = Encoding.UTF8.GetBytes(secret);
            }
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            return Task.FromResult(new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserId = user.Id,
                Email = user.Email ?? "",
                Name = user.Name
            });
        }
    }
}
