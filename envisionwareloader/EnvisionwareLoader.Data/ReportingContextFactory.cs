using Microsoft.EntityFrameworkCore.Design;

namespace EnvisionwareLoader.Data
{
    class ReportingContextFactory : IDesignTimeDbContextFactory<ReportingContext>
    {
        public ReportingContext CreateDbContext(string[] args)
        {
            return new ReportingContext("Server=(localdb)\\mssqllocaldb;Database=ComputerUsage;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}
