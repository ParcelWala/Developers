using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string refresh_token { get; set; } = string.Empty;
    }

    public class RefreshTokenResponse
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public TokenData? Data { get; set; }
    }

    public class TokenData
    {
        public string access_token { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
    }
}
