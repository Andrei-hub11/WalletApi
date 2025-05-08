using Microsoft.AspNetCore.Identity;

using WalletAPI.Data;
using WalletAPI.Data.Entities;

namespace WalletAPI.Extensions;

public static class IdentityConfig
{
    public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            // configurações de senha
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 2;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            // configurações de bloqueio
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // configurações de usuário
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        });

        return services;
    }
}