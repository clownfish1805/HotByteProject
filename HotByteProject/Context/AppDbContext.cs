using HotByteProject.DTO;
using HotByteProject.Models;
using Microsoft.EntityFrameworkCore;

namespace HotByteProject.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Precision settings
            modelBuilder.Entity<Menu>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // ------------------------
            // Relationships
            // ------------------------

            // Restaurant <-> User
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Menu <-> Restaurant
            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Restaurant)
                .WithMany(r => r.Menus)
                .HasForeignKey(m => m.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Menu <-> Category
            modelBuilder.Entity<Menu>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Menus)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // CartItem <-> User
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // CartItem <-> Menu (optional)
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Menu)
                .WithMany()
                .HasForeignKey(c => c.MenuId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // OrderItem <-> Menu (optional)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Menu)
                .WithMany()
                .HasForeignKey(oi => oi.MenuId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // OrderItem <-> Order
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // Order <-> User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------
            // Global Query Filters
            // ------------------------
            modelBuilder.Entity<Menu>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        }

        // Optional: if you plan to implement service-like methods here
        internal async Task<bool> UpdateRestaurantAsync(int restaurantId, RestaurantDTO dto)
        {
            throw new NotImplementedException();
        }
    }
}
