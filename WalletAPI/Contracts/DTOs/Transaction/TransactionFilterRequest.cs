using WalletAPI.Contracts.Enums;

namespace WalletAPI.Contracts.DTOs.Transaction;

public record TransactionFilterRequest(
    string? SenderId,
    string? ReceiverId,
    TransactionType? TransactionType,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page = 1,
    int PageSize = 10
);