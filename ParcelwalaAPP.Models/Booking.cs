using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.Models
{
    
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        public int CustomerID { get; set; }

        [Required]
        public int VehicleTypeId { get; set; }
        public int DriverID { get; set; }
        public int VehicleID { get; set; }

        [Required]
        [MaxLength(20)]
        public string BookingNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,6)")]
        public decimal PickupLatitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,6)")]
        public decimal PickupLongitude { get; set; }

        [MaxLength(500)]
        public string PickupAddress { get; set; } = string.Empty;
        public string? PickupCity { get; set; } = string.Empty;
        public string? PickupContactName { get; set; } = string.Empty;
        public string? PickupContactPhone { get; set; } = string.Empty;

        public DateTime? PickupDateTime { get; set; }   
        public DateTime? ScheduledPickupTime { get; set; }   


        [Required]
        [Column(TypeName = "decimal(10,6)")]
        public decimal DropLatitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,6)")]
        public decimal DropLongitude { get; set; }

        [MaxLength(500)]
        public string DropAddress { get; set; } = string.Empty;
        public string DropCity { get; set; } = string.Empty;
        public string DropContactName { get; set; } = string.Empty;
        public string DropContactPhone { get; set; } = string.Empty;
        public string ActualDropTime { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal EstimatedDistance { get; set; }

        public int EstimatedDurationMinutes { get; set; }  //EstimatedDuration
        public int ActualDuration { get; set; }
        

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BaseFare { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DistanceFare { get; set; }
        public decimal TimeFare { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LoadingCharges { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnloadingCharges { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal WaitingCharges { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TollCharges { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PlatformFee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SurgeMultiplier { get; set; } = 1.0m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal SurgeAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal GstPercentage { get; set; } = 5.0m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal GstAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalFare { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal RoundedFare { get; set; }

        [MaxLength(20)]
        public string? BookingStatus { get; set; } 
        public string? PaymentStatus { get; set; }
        public string? CancellationReason { get; set; }
        public string? CancelledBy { get; set; }

        public DateTime? CancelledAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; } = DateTime.UtcNow;

        public string? GoodsType { get; set; }
        public decimal? GoodsWeight { get; set; }

        public decimal? RequiresLoading { get; set; }
        public decimal? RequiresUnloading { get; set; }

        public string? SpecialInstructions { get; set; }
        public string? PromoCode { get; set; }
        public bool? IsScheduled { get; set; }
        public string? ReferenceNumber { get; set; }
        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual CustomerProfiles Customer { get; set; } = null!;

        [ForeignKey("VehicleTypeId")]
        public virtual VehicleTypes VehicleType { get; set; } = null!;
    }
}
