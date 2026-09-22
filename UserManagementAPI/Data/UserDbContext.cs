using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Utilities;

namespace UserManagementAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Utils.User> Users => Set<Utils.User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Utils.User>().HasData(
            // Password sin hashear: "Ana12345"
            new Utils.User { Id = 1, Name = "Ana", LastName = "García", Mail = "ana@example.com", PasswordHash = "$2a$11$qKrxWOhFcENgbJxAZHUU1OKlaPALotDndzVGdc8tiKTeq2WkCZvja"  },
            
            // Password sin hashear: "Luis12345"
            new Utils.User { Id = 2, Name = "Luis", LastName = "Pérez", Mail = "luis@example.com", PasswordHash = "$2a$11$/eoAXGjj9I8wfo9nZZb9C.lU.TObSUaT20/osgrUuvmeeCRcr9KOO" }
        );
    }
}