using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WalletAPI.Contracts.DTOs.Pagination;
using WalletAPI.Contracts.DTOs.Wallet;
using WalletAPI.Services.Interfaces;

namespace WalletAPI.Controllers;

[ApiController]
[Route("api/v1/wallets")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(PaginatedResponse<WalletResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<WalletResponse>>> GetWallets(
        [FromQuery] WalletFilterRequest filter)
    {
        var wallets = await _walletService.GetWalletsAsync(filter);
        return Ok(wallets);
    }

    [HttpGet("user/balance")]
    [Authorize]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletResponse>> GetUserBalance()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var response = await _walletService.GetUserBalanceAsync(userId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("deposit/{userId}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WalletResponse>> AddBalance([FromBody] AddBalanceRequest request, string userId)
    {
        try
        {
            var response = await _walletService.AddBalanceAsync(userId, request);
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