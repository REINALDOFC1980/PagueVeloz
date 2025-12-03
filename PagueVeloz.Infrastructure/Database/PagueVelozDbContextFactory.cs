using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PagueVeloz.TransactionProcessor.Infrastructure.Database
{
    public class PagueVelozDbContextFactory : IDesignTimeDbContextFactory<PagueVelozDbContext>
    {
        public PagueVelozDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PagueVelozDbContext>();

            // Coloque aqui o connection string direto
            var connectionString = "Server=localhost,1433;Database=PagueVeloz;User Id=sa;Password=Aa123456!Passw0rd;TrustServerCertificate=True";

            optionsBuilder.UseSqlServer(connectionString);

            return new PagueVelozDbContext(optionsBuilder.Options);
        }
    }
}
