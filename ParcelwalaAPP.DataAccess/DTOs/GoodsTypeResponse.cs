using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class GoodsTypeDto
    {
        public int GoodsTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public double DefaultWeight { get; set; }
        public int DefaultPackages { get; set; }
        public int DefaultValue { get; set; }
        public bool IsActive { get; set; }
    }

    public class GoodsTypesResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<GoodsTypeDto> Data { get; set; } = new();
    }
}
