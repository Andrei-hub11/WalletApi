using System.ComponentModel.DataAnnotations;

namespace WalletAPI.Contracts.DTOs.Auth;

public record RegisterRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; init; } = string.Empty;
}