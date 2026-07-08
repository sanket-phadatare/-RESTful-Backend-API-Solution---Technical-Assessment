using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public IdentityService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<JwtSettings> jwtSettings)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<UserDto?> RegisterAsync(RegisterRequest request)
        {
            // Check if user already exists
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Username == request.Username);
            if (existingUsers.Any())
            {
                return null; // Username already taken
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Role = request.Role
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<TokenResponse?> LoginAsync(LoginRequest request)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == request.Username);
            var user = users.FirstOrDefault();

            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(user.Id);

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.CompleteAsync();

            return new TokenResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<TokenResponse?> RefreshTokenAsync(string token)
        {
            var tokens = await _unitOfWork.RefreshTokens.FindAsync(t => t.Token == token);
            var refreshToken = tokens.FirstOrDefault();

            if (refreshToken == null || !refreshToken.IsActive)
            {
                return null; // Invalid/expired token
            }

            var users = await _unitOfWork.Users.FindAsync(u => u.Id == refreshToken.UserId);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            // Rotate refresh token
            var newRefreshToken = GenerateRefreshToken(user.Id);
            
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            _unitOfWork.RefreshTokens.Update(refreshToken);
            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
            await _unitOfWork.CompleteAsync();

            var jwtToken = GenerateJwtToken(user);

            return new TokenResponse
            {
                AccessToken = jwtToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var tokens = await _unitOfWork.RefreshTokens.FindAsync(t => t.Token == token);
            var refreshToken = tokens.FirstOrDefault();

            if (refreshToken == null || !refreshToken.IsActive)
            {
                return false;
            }

            refreshToken.Revoked = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(refreshToken);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(int userId)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                Created = DateTime.UtcNow
            };
        }
    }
}
