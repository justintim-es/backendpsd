
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace backend.Models;

public class Coschon : IdentityDbContext<SC, ShopRole, Guid> {
    public Coschon(DbContextOptions<Coschon> options): base(options)
    {
        
    }
    public DbSet<Shop> Shops { get; set; }
}
public class CoschonFactory : IDesignTimeDbContextFactory<Coschon> {
    public Coschon CreateDbContext(string[] args) {
        var optionsbuilder = new DbContextOptionsBuilder<Coschon>();
        optionsbuilder.UseSqlServer("server=localhost; database=pdb; user id=sa; password=Mischic32!; TrustServerCertificate=true;");
        return new Coschon(optionsbuilder.Options);
    }
}
public class SC : IdentityUser<Guid> {} 

public class Shop : SC {
    public string KVKNumber { get; set; } = "";

    public string CompanyName { get; set; } = "";

    public string StripeId { get; set; } = "";

}
public class Customer : SC {

} 
public class ShopRole : IdentityRole<Guid> {}

