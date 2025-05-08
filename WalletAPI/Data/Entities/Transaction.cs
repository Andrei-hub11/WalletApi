using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using WalletAPI.Contracts.Enums;

namespace WalletAPI.Data.Entities;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;

    [Required]
    public string ReceiverId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TransactionType Type { get; set; } = TransactionType.Transfer;
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

    [StringLength(255)]
    public string Description { get; set; } = string.Empty;

    // Relacionamentos
    [ForeignKey("SenderId")]
    public virtual User Sender { get; set; } = null!;

    [ForeignKey("ReceiverId")]
    public virtual User Receiver { get; set; } = null!;
}