using Microsoft.EntityFrameworkCore;

using WalletAPI.Contracts.DTOs.Wallet;
using WalletAPI.Contracts.Enums;
using WalletAPI.Data;
using WalletAPI.Data.Entities;
using WalletAPI.Services.Interfaces;

namespace WalletAPI.Services.Implementations;

public class WalletService : IWalletService
{
    private readonly ApplicationDbContext _context;

    public WalletService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WalletResponse> GetBalanceAsync(string userId)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new KeyNotFoundException("Carteira não encontrada");

        return new WalletResponse(
            wallet.Balance,
            wallet.UpdatedAt
        );
    }

    public async Task<WalletResponse> AddBalanceAsync(string userId, AddBalanceRequest request)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new KeyNotFoundException("Carteira não encontrada");

        wallet.Balance += request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        // Cria uma transação de depósito
        var transaction = new Transaction
        {
            SenderId = userId,
            ReceiverId = userId,
            Amount = request.Amount,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            Description = "Depósito em conta"
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return new WalletResponse(
            wallet.Balance,
            wallet.UpdatedAt
        );
    }
}