using Microsoft.EntityFrameworkCore;
using PaymentApp.Models;

namespace PaymentApp.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();

}