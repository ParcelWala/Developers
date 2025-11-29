//using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Parcelwala.DataAccess.Data;
using Parcelwala.DataAccess.Services;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.Models;


//using Parcelwala.DataAccess.Data;
//using Parcelwala.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface IVehicleService
    {
        Task<VehicleTypesResponse> GetAllVehicleTypesAsync();
        Task<VehicleTypesResponse> GetAvailableVehicleTypesAsync();

    }

    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehicleService> _logger;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "VehicleTypes_All";
        private const string AvailableCacheKey = "VehicleTypes_Available";
        private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
        // private const int OTP_EXPIRY_MINUTES=5;
        public VehicleService(

            AppDbContext context,
            ILogger<VehicleService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }



        public async Task<VehicleTypesResponse> GetAllVehicleTypesAsync()
        {
            try
            {
                // Try to get from cache first
                if (_cache.TryGetValue(CacheKey, out List<VehicleTypeDto>? cachedTypes)
                    && cachedTypes != null)
                {
                    _logger.LogInformation("Retrieved {Count} vehicle types from cache", cachedTypes.Count);

                    return new VehicleTypesResponse
                    {
                        success = true,
                        message = "Vehicle types retrieved successfully",
                        Data = cachedTypes
                    };
                }
               
                // If not in cache, fetch from database
                var vehicleTypes = await _context.VehicleTypes
                    .OrderBy(v => v.VehicleTypeID)
                    .Select(v => new VehicleTypeDto
                    {
                        vehicle_type_id = v.VehicleTypeID,
                        name = v.DisplayName,
                        icon = v.Icon,
                        description = v.Description,
                        capacity = v.Capacity,
                        base_price = v.BaseFare,
                        free_distance_km = v.FreeDistanceKm,
                        price_per_km = v.PerKmRate,
                        platform_fee = v.PlatformFee,
                        waiting_charge_per_min = v.PerMinuteRate,
                        free_waiting_time_mins = v.FreeWaitingTimeMins,
                        min_fare = v.MinimumFare,
                        max_capacity_kg = v.MaxCapacityKg,
                        dimensions = v.Dimensions,
                        is_available = v.IsAvailable,
                        image_url = v.ImageURL,
                        surge_enabled = v.SurgeEnabled
                    })
                    .ToListAsync();

                if (vehicleTypes.Count == 0)
                {
                    _logger.LogWarning("No vehicle types found in database");

                    return new VehicleTypesResponse
                    {
                        success = false,
                        message = "No vehicle types found",
                        Data = new List<VehicleTypeDto>()
                    };
                }

                // Cache the results
                _cache.Set(CacheKey, vehicleTypes, CacheDuration);

                _logger.LogInformation("Retrieved and cached {Count} vehicle types from database",
                    vehicleTypes.Count);

                return new VehicleTypesResponse
                {
                    success = true,
                    message = "Vehicle types retrieved successfully",
                    Data = vehicleTypes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle types");

                return new VehicleTypesResponse
                {
                    success = false,
                    message = "An error occurred while retrieving vehicle types",
                    Data = new List<VehicleTypeDto>()
                };
            }
        }

        public async Task<VehicleTypesResponse> GetAvailableVehicleTypesAsync()
        {
            try
            {
                // Try to get from cache first
                if (_cache.TryGetValue(AvailableCacheKey, out List<VehicleTypeDto>? cachedTypes)
                    && cachedTypes != null)
                {
                    _logger.LogInformation("Retrieved {Count} available vehicle types from cache",
                        cachedTypes.Count);

                    return new VehicleTypesResponse
                    {
                        success = true,
                        message = "Available vehicle types retrieved successfully",
                        Data = cachedTypes
                    };
                }

                // If not in cache, fetch from database
                var vehicleTypes = await _context.VehicleTypes
                    .Where(v => v.IsAvailable)
                    .OrderBy(v => v.VehicleTypeID)
                    .Select(v => new VehicleTypeDto
                    {
                        vehicle_type_id = v.VehicleTypeID,
                        name = v.DisplayName,
                        icon = v.Icon,
                        description = v.Description,
                        capacity = v.Capacity,
                        base_price = v.BaseFare,
                        free_distance_km = v.FreeDistanceKm,
                        price_per_km = v.PerKmRate,
                        platform_fee = v.PlatformFee,
                        waiting_charge_per_min = v.PerMinuteRate,
                        free_waiting_time_mins = v.FreeWaitingTimeMins,
                        min_fare = v.MinimumFare,
                        max_capacity_kg = v.MaxCapacityKg,
                        dimensions = v.Dimensions,
                        is_available = v.IsAvailable,
                        image_url = v.ImageURL,
                        surge_enabled = v.SurgeEnabled
                    })
                    .ToListAsync();

                if (vehicleTypes.Count == 0)
                {
                    _logger.LogWarning("No available vehicle types found in database");

                    return new VehicleTypesResponse
                    {
                        success = false,
                        message = "No available vehicle types found",
                        Data = new List<VehicleTypeDto>()
                    };
                }

                // Cache the results
                _cache.Set(AvailableCacheKey, vehicleTypes, CacheDuration);

                _logger.LogInformation("Retrieved and cached {Count} available vehicle types from database",
                    vehicleTypes.Count);

                return new VehicleTypesResponse
                {
                    success = true,
                    message = "Available vehicle types retrieved successfully",
                    Data = vehicleTypes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available vehicle types");

                return new VehicleTypesResponse
                {
                    success = false,
                    message = "An error occurred while retrieving available vehicle types",
                    Data = new List<VehicleTypeDto>()
                };
            }
        }

    }
}
