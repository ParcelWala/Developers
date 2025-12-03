using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class CalculateFareRequest
    {
        [Required(ErrorMessage = "Vehicle type ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Vehicle type ID must be greater than 0")]
        public int vehicle_type_id { get; set; }

        [Required(ErrorMessage = "Pickup latitude is required")]
        [Range(-90, 90, ErrorMessage = "Pickup latitude must be between -90 and 90")]
        public double pickup_latitude { get; set; }

        [Required(ErrorMessage = "Pickup longitude is required")]
        [Range(-180, 180, ErrorMessage = "Pickup longitude must be between -180 and 180")]
        public double pickup_longitude { get; set; }

        [Required(ErrorMessage = "Drop latitude is required")]
        [Range(-90, 90, ErrorMessage = "Drop latitude must be between -90 and 90")]
        public double drop_latitude { get; set; }

        [Required(ErrorMessage = "Drop longitude is required")]
        [Range(-180, 180, ErrorMessage = "Drop longitude must be between -180 and 180")]
        public double drop_longitude { get; set; }
    }

    public class FareBreakdownItem  
    {
        public string label { get; set; } = string.Empty;
        public decimal value { get; set; }
        public string type { get; set; } = string.Empty; // "charge" or "tax"
    }

    public class FareCalculationData
    {
        public decimal base_fare { get; set; }
        public decimal distance_km { get; set; }
        public decimal free_distance_km { get; set; }
        public decimal chargeable_distance_km { get; set; }
        public decimal distance_fare { get; set; }
        public decimal loading_charges { get; set; }
        public int free_loading_time_mins { get; set; }
        public decimal waiting_charges { get; set; }
        public decimal toll_charges { get; set; }
        public decimal platform_fee { get; set; }
        public decimal surge_multiplier { get; set; }
        public decimal surge_amount { get; set; }
        public decimal sub_total { get; set; }
        public decimal gst_percentage { get; set; }
        public decimal gst_amount { get; set; }
        public decimal discount { get; set; }
        public decimal total_fare { get; set; }
        public decimal rounded_fare { get; set; }
        public int estimated_duration_minutes { get; set; }
        public string currency { get; set; } = "INR";
        public string? promo_applied { get; set; }
        public List<FareBreakdownItem> fare_breakdown { get; set; } = new();
    }

    public class CalculateFareResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public FareCalculationData? data { get; set; }
    }
}
