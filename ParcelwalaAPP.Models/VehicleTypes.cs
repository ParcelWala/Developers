using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.Models
{
    [Table("VehicleTypes")]
    public class VehicleTypes
    {
        [Key]
      public int  VehicleTypeID     {get;set;}
      public string  TypeName          {get;set;}
      public string  DisplayName       {get;set;}
      public string?  Description       {get;set;}
      public string?  ImageURL          {get;set;}
        public string?  Icon          {get;set;}
      public string  Capacity          {get;set;}
      public string?  Dimensions        {get;set;}
      public decimal  BaseFare          {get;set;}
      public decimal FreeDistanceKm { get;set;}
      public decimal  PerKmRate         {get;set;}
      public decimal?  PerMinuteRate     {get;set;}
      public decimal? FreeWaitingTimeMins { get;set;}
      public decimal? PlatformFee { get;set;}
      public decimal  MinimumFare       {get;set;}
      public decimal MaxCapacityKg { get;set;}
      public decimal?  LoadingCharges    {get;set;}
      public decimal? UnloadingCharges  {get;set;}
      public bool?  IsActive          {get;set;}
      public bool IsAvailable { get;set;}
      public bool? SurgeEnabled { get;set;}
      public int?  DisplayOrder      {get;set;}
      public DateTime?  CreatedAt   {get;set;}
      public DateTime? UpdatedAt { get; set; }
    }
}
