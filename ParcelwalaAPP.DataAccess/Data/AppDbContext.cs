
//using Parcelwala.Models;
using Microsoft.EntityFrameworkCore;
using ParcelwalaAPP;
using ParcelwalaAPP.Models;

namespace Parcelwala.DataAccess.Data
{
	public class AppDbContext:DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
				
		}
        //public DbSet<ActivityLogs> ActivityLogs { get; set; }
        //public DbSet<AdminProfiles> AdminProfiles { get; set; }
        //public DbSet<AppSettings> AppSettings { get; set; }
        //public DbSet<Banners> Banners { get; set; }
        //public DbSet<BlogPosts> BlogPosts { get; set; }
        //public DbSet<Bookings> Bookings { get; set; }
        //public DbSet<BookingStatusHistory> BookingStatusHistory { get; set; }
        //public DbSet<BookingStops> BookingStops { get; set; }
        //public DbSet<ChatConversations> ChatConversations { get; set; }
        //public DbSet<ChatMessages> ChatMessages { get; set; }
        //public DbSet<CommissionRates> CommissionRates { get; set; }
        public DbSet<CustomerProfiles> CustomerProfiles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        //public DbSet<DriverDocuments> DriverDocuments { get; set; }
        //public DbSet<DriverEarnings> DriverEarnings { get; set; }
        //public DbSet<DriverProfiles> DriverProfiles { get; set; }
        //public DbSet<EmergencyContacts> EmergencyContacts { get; set; }
        //public DbSet<FAQCategories> FAQCategories { get; set; }
        //public DbSet<FAQs> FAQs { get; set; }
        //public DbSet<Invoices> Invoices { get; set; }
        //public DbSet<LiveTracking> LiveTracking { get; set; }
        //public DbSet<Notifications> Notifications { get; set; }
        //public DbSet<NotificationTemplates> NotificationTemplates { get; set; }
        public DbSet<OTPVerifications> OTPVerifications { get; set; }
        //public DbSet<PageContent> PageContent { get; set; }
        //public DbSet<PaymentMethods> PaymentMethods { get; set; }
        //public DbSet<PricingZones> PricingZones { get; set; }
        //public DbSet<PromoCodes> PromoCodes { get; set; }
        //public DbSet<PromoCodeUsage> PromoCodeUsage { get; set; }
        //public DbSet<Ratings> Ratings { get; set; }
        //public DbSet<Referrals> Referrals { get; set; }
        //public DbSet<SafetyAlerts> SafetyAlerts { get; set; }
        //public DbSet<ServiceAreas> ServiceAreas { get; set; }
        //public DbSet<SupportMessages> SupportMessages { get; set; }
        //public DbSet<SupportTickets> SupportTickets { get; set; }
        //public DbSet<Transactions> Transactions { get; set; }
        //public DbSet<UserAddresses> UserAddresses { get; set; }
        public DbSet<Users> Users { get; set; }
        //public DbSet<VehicleDocuments> VehicleDocuments { get; set; }
        //public DbSet<Vehicles> Vehicles { get; set; }
        public DbSet<VehicleTypes> VehicleTypes { get; set; }
        public DbSet<WalletTransactions> WalletTransaction { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //// Configure unique index for email
            //modelBuilder.Entity<Users>()
            //    .HasIndex(u => u.Email)
            //    .IsUnique();

            //// Configure unique index for phone number
            //modelBuilder.Entity<Users>()
            //    .HasIndex(u => u.PhoneNumber)
            //    .IsUnique();

            // VehicleType configuration
            //modelBuilder.Entity<VehicleTypes>(entity =>
            //{
            //    entity.HasIndex(e => e.DisplayName);
            //    entity.HasIndex(e => e.IsAvailable);

            //    entity.Property(e => e.CreatedAt)
            //        .HasDefaultValueSql("GETUTCDATE()");

            //    entity.Property(e => e.BaseFare)
            //        .HasDefaultValue(0);

            //    entity.Property(e => e.FreeDistanceKm)
            //        .HasDefaultValue(2.0);

            //    entity.Property(e => e.PricePerKm)
            //        .HasDefaultValue(8.0);

            //    entity.Property(e => e.PlatformFee)
            //        .HasDefaultValue(5);

            //    entity.Property(e => e.WaitingChargePerMin)
            //        .HasDefaultValue(1.5);

            //    entity.Property(e => e.FreeWaitingTimeMins)
            //        .HasDefaultValue(15);

            //    // Seed data
            //    entity.HasData(
            //        new VehicleTypes
            //        {
            //            VehicleTypeId = 1,
            //            Name = "2 Wheeler",
            //            Icon = "🏍️",
            //            Description = "Perfect for small packages",
            //            Capacity = "Up to 10 kg",
            //            BasePrice = 50,
            //            FreeDistanceKm = 2.0,
            //            PricePerKm = 8.0,
            //            PlatformFee = 5,
            //            WaitingChargePerMin = 1.5,
            //            FreeWaitingTimeMins = 15,
            //            MinFare = 50,
            //            MaxCapacityKg = 10,
            //            Dimensions = "45cm x 35cm x 30cm",
            //            IsAvailable = true,
            //            ImageUrl = "https://api.parcelwala.com/images/vehicles/bike.png",
            //            SurgeEnabled = false,
            //            CreatedAt = DateTime.UtcNow
            //        },
            //        new VehicleTypes
            //        {
            //            VehicleTypeId = 2,
            //            Name = "3 Wheeler",
            //            Icon = "🛺",
            //            Description = "Ideal for medium loads",
            //            Capacity = "Up to 100 kg",
            //            BasePrice = 80,
            //            FreeDistanceKm = 2.0,
            //            PricePerKm = 10.0,
            //            PlatformFee = 8,
            //            WaitingChargePerMin = 2.0,
            //            FreeWaitingTimeMins = 20,
            //            MinFare = 80,
            //            MaxCapacityKg = 100,
            //            Dimensions = "120cm x 90cm x 90cm",
            //            IsAvailable = true,
            //            ImageUrl = "https://api.parcelwala.com/images/vehicles/auto.png",
            //            SurgeEnabled = true,
            //            CreatedAt = DateTime.UtcNow
            //        },
            //        new VehicleTypes
            //        {
            //            VehicleTypeId = 3,
            //            Name = "4 Wheeler (Small)",
            //            Icon = "🚗",
            //            Description = "For larger packages",
            //            Capacity = "Up to 300 kg",
            //            BasePrice = 150,
            //            FreeDistanceKm = 3.0,
            //            PricePerKm = 12.0,
            //            PlatformFee = 10,
            //            WaitingChargePerMin = 2.5,
            //            FreeWaitingTimeMins = 25,
            //            MinFare = 150,
            //            MaxCapacityKg = 300,
            //            Dimensions = "150cm x 120cm x 100cm",
            //            IsAvailable = true,
            //            ImageUrl = "https://api.parcelwala.com/images/vehicles/car.png",
            //            SurgeEnabled = true,
            //            CreatedAt = DateTime.UtcNow
            //        },
            //        new VehicleTypes
            //        {
            //            VehicleTypeId = 4,
            //            Name = "Pickup Truck",
            //            Icon = "🚙",
            //            Description = "For heavy and bulk items",
            //            Capacity = "Up to 1000 kg",
            //            BasePrice = 300,
            //            FreeDistanceKm = 5.0,
            //            PricePerKm = 15.0,
            //            PlatformFee = 15,
            //            WaitingChargePerMin = 3.0,
            //            FreeWaitingTimeMins = 30,
            //            MinFare = 300,
            //            MaxCapacityKg = 1000,
            //            Dimensions = "200cm x 150cm x 150cm",
            //            IsAvailable = true,
            //            ImageUrl = "https://api.parcelwala.com/images/vehicles/pickup.png",
            //            SurgeEnabled = true,
            //            CreatedAt = DateTime.UtcNow
            //        }
            //    );
            //});
        }


    }
}
