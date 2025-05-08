namespace WalletAPI.Extensions;

public static class AuthorizationConfig
{
    public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("User", policy => policy.RequireRole("User"));
            options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
        });

        return services;
    }
}