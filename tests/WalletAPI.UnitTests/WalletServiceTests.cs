using Microsoft.EntityFrameworkCore;

using Shouldly;

using WalletAPI.Contracts.DTOs.Wallet;
using WalletAPI.Contracts.Enums;
using WalletAPI.Data;
using WalletAPI.Data.Entities;
using WalletAPI.Services.Implementations;
using WalletAPI.Services.Interfaces;

namespace WalletAPI.UnitTests;

public class WalletServiceTests
{
    private IWalletService GetWalletService(out ApplicationDbContext context)
    {
        // gerando um novo database único a cada chamada, para evitar interferências em testes 
        var _accountOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
           .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
           .Options;
        context = new ApplicationDbContext(_accountOptions);

        return new WalletService(context);
    }

    [Fact]
    public async Task GetWalletsAsync_ShouldReturnFilteredAndPaginatedWallets()
    {
        // Arrange
        var service = GetWalletService(out var context);
        var wallets = new List<Wallet>();
        var now = DateTime.UtcNow;

        // Criar 20 carteiras com valores diferentes
        for (int i = 0; i < 20; i++)
        {
            var wallet = new Wallet
            {
                UserId = $"user-{i}",
                Balance = i * 100,
                CreatedAt = now.AddDays(-i),
                UpdatedAt = now.AddHours(-i)
            };
            wallets.Add(wallet);
        }

        await context.Wallets.AddRangeAsync(wallets);
        await context.SaveChangesAsync();

        var filter = new WalletFilterRequest(
            UserId: null,
            MinBalance: 500,
            MaxBalance: 1500,
            CreatedStartDate: now.AddDays(-10),
            CreatedEndDate: now,
            UpdatedStartDate: null,
            UpdatedEndDate: null,
            Page: 1,
            PageSize: 5
        );

        // Act
        var result = await service.GetWalletsAsync(filter);

        // Assert
        result.Items.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(6);
        result.PageSize.ShouldBe(5);
        result.PageNumber.ShouldBe(1);
        result.Items.All(w => w.Balance >= 500 && w.Balance <= 1500).ShouldBeTrue();
    }

    [Fact]
    public async Task GetWalletsAsync_ShouldReturnEmptyList_WhenNoWalletsMatchFilters()
    {
        // Arrange
        var service = GetWalletService(out var context);
        var filter = new WalletFilterRequest(
            UserId: "non-existent-user",
            MinBalance: null,
            MaxBalance: null,
            CreatedStartDate: null,
            CreatedEndDate: null,
            UpdatedStartDate: null,
            UpdatedEndDate: null,
            Page: 1,
            PageSize: 10
        );

        // Act
        var result = await service.GetWalletsAsync(filter);

        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetUserBalanceAsync_ShouldReturnWallet_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var wallet = new Wallet
        {
            UserId = userId,
            Balance = 100,
            UpdatedAt = DateTime.UtcNow
        };

        var service = GetWalletService(out var context);
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync();

        // Act
        var response = await service.GetUserBalanceAsync(userId);

        // Assert
        response.Balance.ShouldBe(100);
        response.LastUpdate.ShouldBe(wallet.UpdatedAt);
    }

    [Fact]
    public async Task GetUserBalanceAsync_ShouldThrow_WhenWalletDoesNotExist()
    {
        // Arrange
        var service = GetWalletService(out _);

        // Act & Assert
        await Should.ThrowAsync<KeyNotFoundException>(() =>
            service.GetUserBalanceAsync("non-existing-user-id"));
    }

    [Fact]
    public async Task AddBalanceAsync_ShouldAddAmountAndCreateTransaction_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var initialBalance = 50m;
        var amountToAdd = 25m;
        var updatedAt = DateTime.UtcNow;
        var wallet = new Wallet
        {
            UserId = userId,
            Balance = initialBalance,
            UpdatedAt = updatedAt
        };

        var service = GetWalletService(out var context);
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync();

        var request = new AddBalanceRequest { Amount = amountToAdd };

        // Act
        var result = await service.AddBalanceAsync(userId, request);

        // Assert
        result.Balance.ShouldBe(initialBalance + amountToAdd);
        result.LastUpdate.ShouldBeGreaterThan(updatedAt);

        var transaction = await context.Transactions.FirstOrDefaultAsync();
        transaction.ShouldNotBeNull();
        transaction.Amount.ShouldBe(amountToAdd);
        transaction.Type.ShouldBe(TransactionType.Deposit);
        transaction.Status.ShouldBe(TransactionStatus.Completed);
        transaction.SenderId.ShouldBe(userId);
        transaction.ReceiverId.ShouldBe(userId);
    }

    [Fact]
    public async Task AddBalanceAsync_ShouldThrow_WhenWalletDoesNotExist()
    {
        // Arrange
        var service = GetWalletService(out _);
        var request = new AddBalanceRequest { Amount = 100m };

        // Act & Assert
        await Should.ThrowAsync<KeyNotFoundException>(() =>
            service.AddBalanceAsync("non-existing-user-id", request));
    }
}