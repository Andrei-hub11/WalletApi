using WalletAPI.Contracts.DTOs.Auth;

namespace WalletAPI.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
}