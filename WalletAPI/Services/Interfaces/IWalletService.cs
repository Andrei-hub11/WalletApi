using WalletAPI.Contracts.DTOs.Wallet;

namespace WalletAPI.Services.Interfaces;

public interface IWalletService
{
    Task<WalletResponse> GetBalanceAsync(string userId);
    Task<WalletResponse> AddBalanceAsync(string userId, AddBalanceRequest request);
}