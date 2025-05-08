using Microsoft.EntityFrameworkCore;

using WalletAPI.Contracts.DTOs.Pagination;
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

    public async Task<PaginatedResponse<WalletResponse>> GetWalletsAsync(WalletFilterRequest filter)
    {
        var query = _context.Wallets
            .Include(w => w.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.UserId))
            query = query.Where(w => w.UserId == filter.UserId);

        if (filter.MinBalance.HasValue)
            query = query.Where(w => w.Balance >= filter.MinBalance.Value);

        if (filter.MaxBalance.HasValue)
            query = query.Where(w => w.Balance <= filter.MaxBalance.Value);

        if (filter.CreatedStartDate.HasValue)
            query = query.Where(w => w.CreatedAt >= filter.CreatedStartDate.Value);

        if (filter.CreatedEndDate.HasValue)
            query = query.Where(w => w.CreatedAt <= filter.CreatedEndDate.Value);

        if (filter.UpdatedStartDate.HasValue)
            query = query.Where(w => w.UpdatedAt >= filter.UpdatedStartDate.Value);

        if (filter.UpdatedEndDate.HasValue)
            query = query.Where(w => w.UpdatedAt <= filter.UpdatedEndDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.UpdatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(w => new WalletResponse(
                w.Balance,
                w.UpdatedAt
            ))
            .ToListAsync();

        return new PaginatedResponse<WalletResponse>(
            items,
            filter.PageSize,
            filter.Page,
            totalCount);
    }

    public async Task<WalletResponse> GetUserBalanceAsync(string userId)
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