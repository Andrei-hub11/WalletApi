using WalletAPI.Data;

namespace WalletAPI.Extensions;

public static class SeedDataExtensions
{
    public static async void SeedDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var services = serviceScope.ServiceProvider;

        try
        {
            await SeedData.Initialize(services);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ocorreu um erro durante a inicialização do banco de dados.");
        }
    }
}