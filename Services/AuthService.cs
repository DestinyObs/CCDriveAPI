using TheDriveAPI.DTOs.Auth;
using TheDriveAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace TheDriveAPI.Services
{
    using TheDriveAPI.Data;
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
            await GenerateAndSendOtp(user);
            return await GenerateAuthResponse(user);
        }

        public async Task<bool> ResendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            await GenerateAndSendOtp(user);
            return true;
        }

        private async Task GenerateAndSendOtp(User user)
        {
            // Remove any existing, unexpired OTPs for this user
            var now = DateTime.UtcNow;
            var existingOtps = _db.OTPs.Where(o => o.UserId == user.Id && !o.Used && o.ExpiresAt > now);
            _db.OTPs.RemoveRange(existingOtps);
            // Generate new OTP
            var otp = new Random().Next(100000, 999999).ToString();
            var otpEntity = new OTP { UserId = user.Id, Otp = otp, ExpiresAt = now.AddMinutes(30), Used = false };
            _db.OTPs.Add(otpEntity);
            await _db.SaveChangesAsync();
            // Send OTP via SendGrid
            var sendGridApiKey = _config["SendGrid:ApiKey"];
            var fromEmail = _config["SendGrid:FromEmail"] ?? "noreply@thedrive.com";
            var fromName = _config["SendGrid:FromName"] ?? "TheDrive";
            var client = new SendGrid.SendGridClient(sendGridApiKey);
            var from = new SendGrid.Helpers.Mail.EmailAddress(fromEmail, fromName);
            var to = new SendGrid.Helpers.Mail.EmailAddress(user.Email, user.Name);
            var subject = "Your OTP Code";
            var plainTextContent = $"Your OTP code is: {otp}";
            var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, null);
            await client.SendEmailAsync(msg);
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
            // Send password reset email via SendGrid
            var sendGridApiKey2 = _config["SendGrid:ApiKey"];
            var fromEmail2 = _config["SendGrid:FromEmail"] ?? "noreply@thedrive.com";
            var fromName2 = _config["SendGrid:FromName"] ?? "TheDrive";
            var client2 = new SendGrid.SendGridClient(sendGridApiKey2);
            var from2 = new SendGrid.Helpers.Mail.EmailAddress(fromEmail2, fromName2);
            var to2 = new SendGrid.Helpers.Mail.EmailAddress(user.Email, user.Name);
            var subject2 = "Password Reset";
            var plainTextContent2 = $"Your password reset token is: {token}";
            var msg2 = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(from2, to2, subject2, plainTextContent2, null);
            await client2.SendEmailAsync(msg2);
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
