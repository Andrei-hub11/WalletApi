using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using WalletAPI.Data.Entities;

namespace WalletAPI.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Wallet - Um usuário tem apenas uma carteira
        modelBuilder.Entity<User>()
            .HasOne(u => u.Wallet)
            .WithOne(w => w.User)
            .HasForeignKey<Wallet>(w => w.UserId);

        // Transações - Relações com usuários
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Sender)
            .WithMany(u => u.SentTransactions)
            .HasForeignKey(t => t.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Receiver)
            .WithMany(u => u.ReceivedTransactions)
            .HasForeignKey(t => t.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}