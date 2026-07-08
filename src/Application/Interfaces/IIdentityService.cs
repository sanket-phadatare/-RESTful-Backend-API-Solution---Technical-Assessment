using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IIdentityService
    {
        Task<UserDto?> RegisterAsync(RegisterRequest request);
        Task<TokenResponse?> LoginAsync(LoginRequest request);
        Task<TokenResponse?> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
    }
}
