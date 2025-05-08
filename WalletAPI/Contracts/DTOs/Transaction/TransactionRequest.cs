using System.ComponentModel.DataAnnotations;

using WalletAPI.Contracts.Enums;

namespace WalletAPI.Contracts.DTOs.Transaction;

public record TransactionRequest
{
    [Required]
    public string ReceiverId { get; init; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
    public decimal Amount { get; init; }

    public string Description { get; init; } = string.Empty;

    public TransactionType Type { get; init; } = TransactionType.Transfer;
}