using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class AuthData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; } // in seconds
        public Customer Customer { get; set; } = null!;
    }
}
