namespace WalletAPI.Contracts.DTOs.Wallet;

public record WalletFilterRequest(
    string? UserId,
    decimal? MinBalance,
    decimal? MaxBalance,
    DateTime? CreatedStartDate,
    DateTime? CreatedEndDate,
    DateTime? UpdatedStartDate,
    DateTime? UpdatedEndDate,
    int Page = 1,
    int PageSize = 10
);