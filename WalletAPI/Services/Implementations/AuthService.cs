using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using WalletAPI.Contracts.DTOs.Auth;
using WalletAPI.Contracts.DTOs.Transaction;
using WalletAPI.Contracts.DTOs.Wallet;
using WalletAPI.Data;
using WalletAPI.Data.Entities;
using WalletAPI.Services.Interfaces;

namespace WalletAPI.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<User> userManager,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Email ou senha inválidos");

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
            throw new UnauthorizedAccessException("Email ou senha inválidos");

        var token = await GenerateJwtToken(user);

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == user.Id);

        var recentTransactions = await _context.Transactions
            .Where(t => t.SenderId == user.Id || t.ReceiverId == user.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
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

        var walletResponse = new WalletResponse(
            wallet?.Balance ?? 0,
            wallet?.UpdatedAt ?? DateTime.UtcNow
        );

        return new AuthResponse(
            user.Email!,
            user.Name,
            token,
            walletResponse,
            recentTransactions);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new InvalidOperationException($"Erro ao criar usuário: {string.Join(", ", errors)}");
        }

        await _userManager.AddToRoleAsync(user, "User");

        var token = await GenerateJwtToken(user);

        var walletResponse = new WalletResponse(
            0,
            DateTime.UtcNow
        );

        return new AuthResponse(
            user.Email,
            user.Name,
            token,
            walletResponse,
            []);
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT Secret Key não configurada");

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.Name)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = Encoding.ASCII.GetBytes(jwtKey);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}