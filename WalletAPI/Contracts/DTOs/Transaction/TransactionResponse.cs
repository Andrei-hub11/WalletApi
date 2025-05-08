using WalletAPI.Contracts.Enums;

namespace WalletAPI.Contracts.DTOs.Transaction;

public record TransactionResponse(
    int Id,
    string SenderId,
    string SenderName,
    string ReceiverId,
    string ReceiverName,
    decimal Amount,
    DateTime CreatedAt,
    TransactionType Type,
    TransactionStatus Status,
    string Description
);