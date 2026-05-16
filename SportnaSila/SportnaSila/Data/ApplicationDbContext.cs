using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportnaSila.Models;

namespace SportnaSila.Data
{
    public class ApplicationDbContext : IdentityDbContext<Clients>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Products> Products { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Suppliers> Suppliers { get; set; }
        public DbSet<Brands> Brands { get; set; }
        public DbSet<Orders> Orders { get; set; }
    }
}
