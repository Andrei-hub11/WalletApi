using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace WalletAPI.Data.Entities;

public class User : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    public virtual Wallet Wallet { get; set; } = new Wallet();
    public virtual ICollection<Transaction> SentTransactions { get; set; } = [];
    public virtual ICollection<Transaction> ReceivedTransactions { get; set; } = [];
}