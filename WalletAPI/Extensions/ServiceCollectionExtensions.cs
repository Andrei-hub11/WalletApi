using WalletAPI.Services.Implementations;
using WalletAPI.Services.Interfaces;

namespace WalletAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}