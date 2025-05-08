using Microsoft.EntityFrameworkCore;

using Shouldly;

using WalletAPI.Contracts.DTOs.Transaction;
using WalletAPI.Contracts.Enums;
using WalletAPI.Data;
using WalletAPI.Data.Entities;
using WalletAPI.Services.Implementations;
using WalletAPI.Services.Interfaces;

namespace WalletAPI.UnitTests;

public class TransactionServiceTests
{
    private ITransactionService GetTransactionService(out ApplicationDbContext context)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new ApplicationDbContext(options);
        return new TransactionService(context);
    }

    [Fact]
    public async Task GetUserTransactionsByUserIdAsync_ShouldReturnTransaction_WhenExists()
    {
        var senderId = "user1";
        var receiverId = "user2";

        var service = GetTransactionService(out var context);

        context.Users.AddRange(
            new User { Id = senderId, Name = "Alice" },
            new User { Id = receiverId, Name = "Bob" });

        var transaction = new Transaction
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Amount = 20,
            Type = TransactionType.Transfer,
            Status = TransactionStatus.Completed,
            Description = "Exemplo"
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var result = await service.GetUserTransactionsByUserIdAsync(senderId);

        result.ShouldNotBeNull();
        result.SenderId.ShouldBe(senderId);
        result.ReceiverId.ShouldBe(receiverId);
        result.Amount.ShouldBe(20m);
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldReturnPaginatedList_WithFilters()
    {
        var senderId = "user1";
        var receiverId = "user2";

        var service = GetTransactionService(out var context);

        context.Users.AddRange(
            new User { Id = senderId, Name = "Sender" },
            new User { Id = receiverId, Name = "Receiver" });

        context.Transactions.AddRange(
            new Transaction
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Amount = 10,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                Description = "1"
            },
            new Transaction
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Amount = 20,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                Description = "2"
            });

        await context.SaveChangesAsync();

        var filter = new TransactionFilterRequest(
            senderId,
            null,
            null,
            null,
            null,
            1,
            10
        );

        var result = await service.GetTransactionsAsync(filter);

        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(2);
        result.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task CreateTransactionAsync_ShouldCreateTransactionAndUpdateWallets_WhenValid()
    {
        // Arrange
        var senderId = Guid.NewGuid().ToString();
        var receiverId = Guid.NewGuid().ToString();

        var sender = new User { Id = senderId, Name = "Sender" };
        var receiver = new User { Id = receiverId, Name = "Receiver" };

        var senderWallet = new Wallet { UserId = senderId, Balance = 100m, UpdatedAt = DateTime.UtcNow };
        var receiverWallet = new Wallet { UserId = receiverId, Balance = 50m, UpdatedAt = DateTime.UtcNow };

        var service = GetTransactionService(out var context);

        context.Users.AddRange(sender, receiver);
        context.Wallets.AddRange(senderWallet, receiverWallet);
        await context.SaveChangesAsync();

        var request = new TransactionRequest
        {
            ReceiverId = receiverId,
            Amount = 25m,
            Type = TransactionType.Transfer,
            Description = "Pagamento"
        };

        // Act
        var result = await service.CreateTransactionAsync(senderId, request);

        // Assert
        result.ShouldNotBeNull();
        result.SenderId.ShouldBe(senderId);
        result.ReceiverId.ShouldBe(receiverId);
        result.Amount.ShouldBe(25m);
        result.Description.ShouldBe("Pagamento");

        var updatedSenderWallet = await context.Wallets.FirstAsync(w => w.UserId == senderId);
        updatedSenderWallet.Balance.ShouldBe(75m);

        var updatedReceiverWallet = await context.Wallets.FirstAsync(w => w.UserId == receiverId);
        updatedReceiverWallet.Balance.ShouldBe(75m);
    }

    [Fact]
    public async Task CreateTransactionAsync_ShouldThrow_WhenSenderHasInsufficientBalance()
    {
        // Arrange
        var senderId = "user1";
        var receiverId = "user2";

        var service = GetTransactionService(out var context);

        context.Users.AddRange(
            new User { Id = senderId, Name = "Sender" },
            new User { Id = receiverId, Name = "Receiver" });

        context.Wallets.AddRange(
            new Wallet { UserId = senderId, Balance = 5m },
            new Wallet { UserId = receiverId, Balance = 50m });

        await context.SaveChangesAsync();

        var request = new TransactionRequest
        {
            ReceiverId = receiverId,
            Amount = 10m,
            Type = TransactionType.Transfer,
            Description = "Tentativa inválida"
        };

        // Act + Assert
        await Should.ThrowAsync<InvalidOperationException>(() =>
            service.CreateTransactionAsync(senderId, request));
    }

    [Fact]
    public async Task CreateTransactionAsync_ShouldThrow_WhenReceiverNotFound()
    {
        var userId = "user1";
        var service = GetTransactionService(out var context);

        context.Users.Add(new User { Id = userId, Name = "User" });
        context.Wallets.Add(new Wallet { UserId = userId, Balance = 100m });
        await context.SaveChangesAsync();

        var request = new TransactionRequest
        {
            ReceiverId = "nonexistent",
            Amount = 10m,
            Type = TransactionType.Transfer
        };

        await Should.ThrowAsync<KeyNotFoundException>(() =>
            service.CreateTransactionAsync(userId, request));
    }
}