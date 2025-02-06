using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DIplomServer
{
    public static class EntityFrameworkInstaller
    {
        public static async Task MigrationDataBaseAsync(this IHost webHost)
        {
            using var scope = webHost.Services.CreateScope();
            var services = scope.ServiceProvider;

            await using var db = services.GetRequiredService<HbtContext>();
            try
            {
                await db.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                //TODO: Add logging
                throw;
            }
        }
    }
}
