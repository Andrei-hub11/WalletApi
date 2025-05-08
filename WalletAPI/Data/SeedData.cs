using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WalletAPI.Contracts.Enums;
using WalletAPI.Data.Entities;

namespace WalletAPI.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await context.Database.MigrateAsync();

            // Verifica se já existem dados no banco
            if (await userManager.Users.AnyAsync())
            {
                logger.LogInformation("O banco de dados já está populado. Seed ignorado.");
                return;
            }

            logger.LogInformation("Iniciando seed do banco de dados...");
            await SeedAdminUser(userManager, roleManager);
            var users = await SeedUsers(userManager);
            await SeedWallets(context, users);
            await SeedTransactions(context, users);
            logger.LogInformation("Seed do banco de dados concluído com sucesso!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu um erro durante o seed do banco de dados.");
            throw;
        }
    }

    private static async Task SeedAdminUser(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        var adminEmail = "admin@email.com";
        var adminPassword = "Admin@123&&";
        var adminRole = "Admin";

        // Criar a role se não existir
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRole));
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Erro ao criar role '{adminRole}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }

        // Verifica se o admin já existe
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin != null)
        {
            // Garante que ele está na role Admin
            if (!await userManager.IsInRoleAsync(existingAdmin, adminRole))
            {
                await userManager.AddToRoleAsync(existingAdmin, adminRole);
            }
            return;
        }

        // Cria o usuário admin
        var admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            Name = "Administrador",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            throw new Exception($"Erro ao criar usuário admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(admin, adminRole);
    }

    private static async Task<List<User>> SeedUsers(UserManager<User> userManager)
    {
        var users = new List<User>();
        var defaultPassword = "Senha@123";

        var seedUsers = new List<(string name, string email)>
        {
            ("João Silva", "joao@email.com"),
            ("Maria Souza", "maria@email.com"),
            ("Carlos Oliveira", "carlos@email.com"),
            ("Ana Santos", "ana@email.com"),
            ("Roberto Pereira", "roberto@email.com")
        };

        foreach (var (name, email) in seedUsers)
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                Name = name,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, defaultPassword);
            if (result.Succeeded)
            {
                users.Add(user);
            }
        }

        return users;
    }

    private static async Task SeedWallets(ApplicationDbContext context, List<User> users)
    {
        var wallets = new List<Wallet>();

        foreach (var (user, index) in users.Select((u, i) => (u, i)))
        {
            wallets.Add(new Wallet
            {
                UserId = user.Id,
                Balance = (index + 1) * 1000 // Saldos iniciais variados: 1000, 2000, 3000, 4000, 5000
            });
        }

        await context.Wallets.AddRangeAsync(wallets);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTransactions(ApplicationDbContext context, List<User> users)
    {
        var transactions = new List<Transaction>();
        var random = new Random();

        // Datas para as transações (últimos 30 dias)
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-30);

        // Gera algumas transferências aleatórias entre usuários
        for (int i = 0; i < 20; i++)
        {
            var senderIndex = random.Next(users.Count);
            var receiverIndex = (senderIndex + 1 + random.Next(users.Count - 1)) % users.Count;

            var sender = users[senderIndex];
            var receiver = users[receiverIndex];

            var amount = Math.Round(50 + (decimal)random.NextDouble() * 150, 2);
            var transactionDate = startDate.AddDays(random.Next(30));

            var transaction = new Transaction
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Amount = amount,
                CreatedAt = transactionDate,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                Description = $"Transferência de {sender.Name} para {receiver.Name}"
            };

            transactions.Add(transaction);
        }

        // Adiciona alguns depósitos
        foreach (var user in users)
        {
            var depositAmount = Math.Round(100 + (decimal)random.NextDouble() * 400, 2);
            var depositDate = startDate.AddDays(random.Next(30));

            var deposit = new Transaction
            {
                SenderId = user.Id,
                ReceiverId = user.Id,
                Amount = depositAmount,
                CreatedAt = depositDate,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                Description = $"Depósito na conta de {user.Name}"
            };

            transactions.Add(deposit);
        }

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();
    }
}