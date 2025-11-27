using Microsoft.EntityFrameworkCore;
using paoecirco.org_server.Domain;

namespace paoecirco.org_server
{
    public class PostgresDbContext : DbContext
    {
        public PostgresDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Attendence> Attendences { get; set; } = null!;
        public DbSet<Councilour> Councilours { get; set; } = null!;
        public DbSet<OfficeSpending> OfficeSpendings { get; set; } = null!;
    }
}
