namespace WalletAPI.Contracts.DTOs.Wallet;

public record WalletResponse(
    decimal Balance,
    DateTime LastUpdate
);