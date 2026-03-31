using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NaturalCandles.Models;
using NaturalCandles.Models.Enums;

namespace NaturalCandles.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ProductPriceTier> ProductPriceTiers { get; set; }
        public DbSet<ShippingMethodSetting> ShippingMethodSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.BasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ProductPriceTier>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ShippingMethodSetting>().HasData(
                new ShippingMethodSetting
                {
                    Id = 1,
                    ShippingMethod = ShippingMethod.InPostLocker,
                    DisplayName = "InPost Paczkomat",
                    Price = 14.99m,
                    IsEnabled = true,
                    RequiresPickupPoint = true,
                    SupportsCashOnDelivery = false,
                    SortOrder = 1
                },
                new ShippingMethodSetting
                {
                    Id = 2,
                    ShippingMethod = ShippingMethod.InPostCourier,
                    DisplayName = "InPost Courier",
                    Price = 16.99m,
                    IsEnabled = true,
                    RequiresPickupPoint = false,
                    SupportsCashOnDelivery = true,
                    SortOrder = 2
                },
                new ShippingMethodSetting
                {
                    Id = 3,
                    ShippingMethod = ShippingMethod.DpdCourier,
                    DisplayName = "DPD Courier",
                    Price = 18.99m,
                    IsEnabled = true,
                    RequiresPickupPoint = false,
                    SupportsCashOnDelivery = true,
                    SortOrder = 3
                },
                new ShippingMethodSetting
                {
                    Id = 4,
                    ShippingMethod = ShippingMethod.OrlenPaczka,
                    DisplayName = "ORLEN Paczka",
                    Price = 12.99m,
                    IsEnabled = true,
                    RequiresPickupPoint = true,
                    SupportsCashOnDelivery = false,
                    SortOrder = 4
                },
                new ShippingMethodSetting
                {
                    Id = 5,
                    ShippingMethod = ShippingMethod.LocalPickup,
                    DisplayName = "Local Pickup",
                    Price = 0m,
                    IsEnabled = true,
                    RequiresPickupPoint = false,
                    SupportsCashOnDelivery = false,
                    SortOrder = 5
                }
            );
        }
    }
}