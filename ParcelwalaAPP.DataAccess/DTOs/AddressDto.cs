using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class AddressResponseDto
    {
        public string address_id { get; set; } = string.Empty;
        public string address_type { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string? landmark { get; set; }
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public string contact_name { get; set; } = string.Empty;
        public string contact_phone { get; set; } = string.Empty;
        public bool is_default { get; set; }
    }

    public class GetAddressesResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public List<AddressResponseDto>? Data { get; set; }
    }

    public class AddAddressRequest
    {
        [Required(ErrorMessage = "Address type is required")]
        [RegularExpression("^(Home|Office|Other)$", ErrorMessage = "Address type must be Home, Office, or Other")]
        public string address_type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Label is required")]
        [MinLength(2, ErrorMessage = "Label must be at least 2 characters")]
        [MaxLength(100, ErrorMessage = "Label cannot exceed 100 characters")]
        public string label { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MinLength(10, ErrorMessage = "Address must be at least 10 characters")]
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string address { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Landmark cannot exceed 200 characters")]
        public string? landmark { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? longitude { get; set; }

        [Required(ErrorMessage = "Contact name is required")]
        [MinLength(2, ErrorMessage = "Contact name must be at least 2 characters")]
        [MaxLength(100, ErrorMessage = "Contact name cannot exceed 100 characters")]
        public string contact_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact phone is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Phone number must be in E.164 format")]
        public string contact_phone { get; set; } = string.Empty;

        public bool is_default { get; set; } = false;
    }
    public class UpdateAddressRequest
    {
        [Required(ErrorMessage = "Address type is required")]
        [RegularExpression("^(Home|Office|Other)$", ErrorMessage = "Address type must be Home, Office, or Other")]
        public string address_type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Label is required")]
        [MinLength(2, ErrorMessage = "Label must be at least 2 characters")]
        [MaxLength(100, ErrorMessage = "Label cannot exceed 100 characters")]
        public string label { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MinLength(10, ErrorMessage = "Address must be at least 10 characters")]
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string address { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Landmark cannot exceed 200 characters")]
        public string? landmark { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? longitude { get; set; }

        [Required(ErrorMessage = "Contact name is required")]
        [MinLength(2, ErrorMessage = "Contact name must be at least 2 characters")]
        [MaxLength(100, ErrorMessage = "Contact name cannot exceed 100 characters")]
        public string contact_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact phone is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Phone number must be in E.164 format")]
        public string contact_phone { get; set; } = string.Empty;

        public bool is_default { get; set; } = false;
    }

    public class UpdateAddressResponse
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public AddressResponseDto? Data { get; set; }
    }
}
