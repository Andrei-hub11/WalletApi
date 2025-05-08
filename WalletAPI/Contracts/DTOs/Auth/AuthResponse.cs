using WalletAPI.Contracts.DTOs.Transaction;
using WalletAPI.Contracts.DTOs.Wallet;

namespace WalletAPI.Contracts.DTOs.Auth;

public record AuthResponse(
    string Email,
    string Name,
    string Token,
    WalletResponse Wallet,
    IEnumerable<TransactionResponse> RecentTransactions
);