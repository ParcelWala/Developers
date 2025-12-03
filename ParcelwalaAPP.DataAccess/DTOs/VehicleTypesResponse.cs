using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.DTOs
{
    public class VehicleTypesResponse
    {
        public bool success { get; set; }   
        public string message { get; set; }
        public List<VehicleTypeDto> Data { get; set; } = new();

    }
    public class VehicleTypeDto
    {
           public int vehicle_type_id         {get;set;}
           public string name                    {get;set;}
           public string? icon                    {get;set;}
           public string? description             {get;set;}
           public string? capacity                {get;set;}
           public decimal base_price              {get;set;}
           public decimal free_distance_km        {get;set;}
           public decimal? price_per_km            {get;set;}
           public decimal? platform_fee            {get;set;}
           public decimal? waiting_charge_per_min  {get;set;}
           public int? free_waiting_time_mins  {get;set;}
           public decimal min_fare                {get;set;}
           public decimal max_capacity_kg         {get;set;}
           public string? dimensions              {get;set;}
           public bool is_available            {get;set;}
           public string? image_url               {get;set;}
           public bool? surge_enabled { get; set; }


    }
}
