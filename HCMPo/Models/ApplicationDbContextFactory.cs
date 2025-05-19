//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Configuration;

//namespace HCMPo.Models
//{
//    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
//    {
//        public ApplicationDbContext CreateDbContext(string[] args)
//        {
//            // Build configuration
//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
//                .Build();

//            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
//            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
//            if (string.IsNullOrEmpty(connectionString))
//            {
//                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
//            }

//            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
//            {
//                sqlOptions.EnableRetryOnFailure(
//                    maxRetryCount: 5,
//                    maxRetryDelay: TimeSpan.FromSeconds(30),
//                    errorNumbersToAdd: null);
//            });

//            return new ApplicationDbContext(optionsBuilder.Options);
//        }
//    }
//} 