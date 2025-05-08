using WalletAPI.Contracts.DTOs.Pagination;
using WalletAPI.Contracts.DTOs.Transaction;

namespace WalletAPI.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> CreateTransactionAsync(string userId, TransactionRequest request);
    Task<PaginatedResponse<TransactionResponse>> GetTransactionsAsync(TransactionFilterRequest filter);
    Task<TransactionResponse> GetUserTransactionsByUserIdAsync(string userId);
}