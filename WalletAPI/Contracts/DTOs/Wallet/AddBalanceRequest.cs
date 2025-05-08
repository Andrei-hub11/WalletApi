using System.ComponentModel.DataAnnotations;

namespace WalletAPI.Contracts.DTOs.Wallet;

public record AddBalanceRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
    public decimal Amount { get; init; }
}