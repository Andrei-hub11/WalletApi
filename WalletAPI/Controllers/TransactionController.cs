using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WalletAPI.Contracts.DTOs.Pagination;
using WalletAPI.Contracts.DTOs.Transaction;
using WalletAPI.Services.Interfaces;

namespace WalletAPI.Controllers;

[ApiController]
[Route("api/v1/transactions")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(PaginatedResponse<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<TransactionResponse>>> GetTransactions(
    [FromQuery] TransactionFilterRequest filter)
    {
        var transactions = await _transactionService.GetTransactionsAsync(filter);
        return Ok(transactions);
    }

    [HttpGet("user")]
    [Authorize]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetUserTransactions()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var transaction = await _transactionService.GetUserTransactionsByUserIdAsync(userId);
            return Ok(transaction);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> CreateTransaction([FromBody] TransactionRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var response = await _transactionService.CreateTransactionAsync(userId, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}