using Microsoft.EntityFrameworkCore;

using WalletAPI.Contracts.DTOs.Pagination;
using WalletAPI.Contracts.DTOs.Transaction;
using WalletAPI.Contracts.Enums;
using WalletAPI.Data;
using WalletAPI.Data.Entities;
using WalletAPI.Services.Interfaces;

namespace WalletAPI.Services.Implementations;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;

    public TransactionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionResponse> CreateTransactionAsync(string userId, TransactionRequest request)
    {
        var receiver = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.ReceiverId)
            ?? throw new KeyNotFoundException("Destinatário não encontrado");

        var senderWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new KeyNotFoundException("Carteira do remetente não encontrada");

        if (senderWallet.Balance < request.Amount)
            throw new InvalidOperationException("Saldo insuficiente");

        var receiverWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == request.ReceiverId)
            ?? throw new KeyNotFoundException("Carteira do destinatário não encontrada");

        var transaction = new Transaction
        {
            SenderId = userId,
            ReceiverId = request.ReceiverId,
            Amount = request.Amount,
            Type = request.Type,
            Status = TransactionStatus.Completed,
            Description = request.Description
        };

        senderWallet.Balance -= request.Amount;
        receiverWallet.Balance += request.Amount;

        senderWallet.UpdatedAt = DateTime.UtcNow;
        receiverWallet.UpdatedAt = DateTime.UtcNow;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var sender = await _context.Users.FirstAsync(u => u.Id == userId);

        return new TransactionResponse(
            transaction.Id,
            transaction.SenderId,
            sender.Name,
            transaction.ReceiverId,
            receiver.Name,
            transaction.Amount,
            transaction.CreatedAt,
            transaction.Type,
            transaction.Status,
            transaction.Description
        );
    }

    public async Task<PaginatedResponse<TransactionResponse>> GetTransactionsAsync(
        TransactionFilterRequest filter)
    {
        var query = _context.Transactions
            .Include(t => t.Sender)
            .Include(t => t.Receiver)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.SenderId))
            query = query.Where(t => t.SenderId == filter.SenderId);

        if (!string.IsNullOrEmpty(filter.ReceiverId))
            query = query.Where(t => t.ReceiverId == filter.ReceiverId);

        if (filter.TransactionType.HasValue)
            query = query.Where(t => t.Type == filter.TransactionType.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.EndDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(t => new TransactionResponse(
                t.Id,
                t.SenderId,
                t.Sender.Name,
                t.ReceiverId,
                t.Receiver.Name,
                t.Amount,
                t.CreatedAt,
                t.Type,
                t.Status,
                t.Description
            ))
            .ToListAsync();

        return new PaginatedResponse<TransactionResponse>(
            items,
            filter.PageSize,
            filter.Page,
            totalCount);
    }

    public async Task<TransactionResponse> GetUserTransactionsByUserIdAsync(string userId)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Sender)
            .Include(t => t.Receiver)
            .FirstOrDefaultAsync(t => t.SenderId == userId || t.ReceiverId == userId)
            ?? throw new KeyNotFoundException("Transação não encontrada");

        return new TransactionResponse(
            transaction.Id,
            transaction.SenderId,
            transaction.Sender.Name,
            transaction.ReceiverId,
            transaction.Receiver.Name,
            transaction.Amount,
            transaction.CreatedAt,
            transaction.Type,
            transaction.Status,
            transaction.Description
        );
    }
}