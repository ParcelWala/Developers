
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
        //public DbSet<VehicleTypes> VehicleTypes { get; set; }
        //public DbSet<WalletTransactions> WalletTransactions { get; set; }


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Configure unique index for email
        //    modelBuilder.Entity<Users>()
        //        .HasIndex(u => u.Email)
        //        .IsUnique();

        //    // Configure unique index for phone number
        //    modelBuilder.Entity<Users>()
        //        .HasIndex(u => u.PhoneNumber)
        //        .IsUnique();
        //}

    }
}
