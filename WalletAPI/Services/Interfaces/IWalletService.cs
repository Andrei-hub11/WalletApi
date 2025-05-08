using WalletAPI.Contracts.DTOs.Pagination;
using WalletAPI.Contracts.DTOs.Wallet;

namespace WalletAPI.Services.Interfaces;

public interface IWalletService
{
    Task<WalletResponse> GetUserBalanceAsync(string userId);
    Task<PaginatedResponse<WalletResponse>> GetWalletsAsync(WalletFilterRequest filter);
    Task<WalletResponse> AddBalanceAsync(string userId, AddBalanceRequest request);
}