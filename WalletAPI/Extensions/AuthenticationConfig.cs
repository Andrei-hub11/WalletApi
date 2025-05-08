using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace WalletAPI.Extensions;

public static class AuthenticationConfig
{
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(jwtKey))
            throw new InvalidOperationException("JWT Secret Key não configurada");

        var key = Encoding.ASCII.GetBytes(jwtKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}