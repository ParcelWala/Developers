using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class RestrictedItemDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class RestrictedItemsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<RestrictedItemDto> Data { get; set; } = new();
    }
}
