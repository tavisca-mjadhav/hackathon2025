using Microsoft.EntityFrameworkCore;
using PaymentApi.Models;

namespace PaymentApi.Repositories
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>().OwnsOne(p => p.Card, card =>
            {
                card.OwnsOne(c => c.Expiry);
            });
        }
    }
}
