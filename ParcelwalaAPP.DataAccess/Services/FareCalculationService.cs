using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Parcelwala.DataAccess.Data;
using ParcelwalaAPP.DataAccess.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface IFareCalculationService
    {
        Task<(bool Success, string Message, FareCalculationData? Data)> CalculateFareAsync(
            int vehicleTypeId,
            double pickupLat,
            double pickupLng,
            double dropLat,
            double dropLng);
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
        int EstimateDuration(double distanceKm);
    }

    public class FareCalculationService : IFareCalculationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FareCalculationService> _logger;
        private readonly IConfiguration _configuration;

        public FareCalculationService(
            AppDbContext context,
            ILogger<FareCalculationService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, FareCalculationData? Data)> CalculateFareAsync(
            int vehicleTypeId,
            double pickupLat,
            double pickupLng,
            double dropLat,
            double dropLng)
        {
            try
            {
                // Get vehicle type
                var vehicleType = await _context.VehicleTypes
                    .FirstOrDefaultAsync(v => v.VehicleTypeID == vehicleTypeId && v.IsActive==true);

                if (vehicleType == null)
                {
                    return (false, "Vehicle type not found or inactive", null);
                }

                // Calculate distance
                var distanceKm = CalculateDistance(pickupLat, pickupLng, dropLat, dropLng);
                var estimatedDuration = EstimateDuration(distanceKm);

                // Calculate fare components
                var baseFare = vehicleType.BaseFare;
                var freeDistanceKm = vehicleType.FreeDistanceKm;
                var chargeableDistanceKm = Math.Max(0, distanceKm - (double)freeDistanceKm);
                var distanceFare = (decimal)chargeableDistanceKm * vehicleType.PerKmRate;

                var loadingCharges = vehicleType.LoadingCharges;
                var waitingCharges = 0m; // No waiting charges for fare estimation
                var tollCharges = 0m; // Toll charges calculated separately

                var subTotal = baseFare + distanceFare + loadingCharges + waitingCharges + tollCharges;

                // Platform fee
                //var platformFee = Math.Round(subTotal * (vehicleType.PlatformFeePercentage / 100), 2);
                var platformFee = vehicleType.PlatformFee;
                subTotal += platformFee;

                // Surge pricing (can be dynamic based on demand)
                var surgeMultiplier = GetSurgeMultiplier();
                var surgeAmount = surgeMultiplier > 1.0m ? Math.Round((decimal)(subTotal * (surgeMultiplier - 1.0m)), 2) : 0m;
                subTotal += surgeAmount;

                // GST calculation
                var gstPercentage = 5.0m;
                var gstAmount = Math.Round((decimal)(subTotal * (gstPercentage / 100)), 2);

                // Discount
                var discount = 0m;

                // Total fare
                var totalFare = subTotal + gstAmount - discount;
                var roundedFare = Math.Ceiling((decimal)(totalFare / 5)) * 5; // Round to nearest 5

                // Build fare breakdown
                var fareBreakdown = new List<FareBreakdownItem>
                {
                    new FareBreakdownItem
                    {
                        label = $"Base Fare (incl. {freeDistanceKm}km)",
                        value = baseFare,
                        type = "charge"
                    }
                };

                if (chargeableDistanceKm > 0)
                {
                    fareBreakdown.Add(new FareBreakdownItem
                    {
                        label = $"Distance Charges ({chargeableDistanceKm:F1}km)",
                        value = distanceFare,
                        type = "charge"
                    });
                }

                if (loadingCharges > 0)
                {
                    fareBreakdown.Add(new FareBreakdownItem
                    {
                        label = "Loading Charges",
                        value = (decimal)loadingCharges,
                        type = "charge"
                    });
                }

                if (platformFee > 0)
                {
                    fareBreakdown.Add(new FareBreakdownItem
                    {
                        label = "Platform Fee",
                        value = (decimal)platformFee,
                        type = "charge"
                    });
                }

                if (surgeAmount > 0)
                {
                    fareBreakdown.Add(new FareBreakdownItem
                    {
                        label = $"Surge Charges ({surgeMultiplier}x)",
                        value = surgeAmount,
                        type = "charge"
                    });
                }

                fareBreakdown.Add(new FareBreakdownItem
                {
                   label = $"GST ({gstPercentage}%)",
                    value = gstAmount,
                    type = "tax"
                });

                if (discount > 0)
                {
                    fareBreakdown.Add(new FareBreakdownItem
                    {
                        label = "Discount",
                        value = -discount,
                        type = "discount"
                    });
                }

                var fareData = new FareCalculationData
                {
                    base_fare = baseFare,
                    distance_km = Math.Round((decimal)distanceKm, 1),
                    free_distance_km = freeDistanceKm,
                    chargeable_distance_km = Math.Round((decimal)chargeableDistanceKm, 1),
                    distance_fare = distanceFare,
                    loading_charges = (decimal)loadingCharges,
                    //free_loading_time_mins = vehicleType.FreeLoadingTimeMinutes,
                    waiting_charges = waitingCharges,
                    toll_charges = tollCharges,
                    platform_fee = (decimal)platformFee,
                    surge_multiplier = surgeMultiplier,
                    surge_amount = surgeAmount,
                    sub_total = (decimal)subTotal,
                    gst_percentage = gstPercentage,
                    gst_amount = gstAmount,
                    discount = discount,
                    total_fare = (decimal)totalFare,
                    rounded_fare = roundedFare,
                    estimated_duration_minutes = estimatedDuration,
                    currency = "INR",
                    promo_applied = null,
                    fare_breakdown = fareBreakdown
                };

                _logger.LogInformation(
                    "Fare calculated for vehicle type {VehicleTypeId}: Distance={Distance}km, Total={Total}",
                    vehicleTypeId, distanceKm, roundedFare);

                return (true, "Fare calculated successfully", fareData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating fare for vehicle type {VehicleTypeId}", vehicleTypeId);
                return (false, "An error occurred while calculating fare", null);
            }
        }

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Haversine formula to calculate distance between two coordinates
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c;

            return Math.Round(distance, 2);
        }

        public int EstimateDuration(double distanceKm)
        {
            // Estimate duration based on average speed
            // Assuming average speed of 25 km/h in urban areas
            var avgSpeedKmh = 25.0;
            var durationHours = distanceKm / avgSpeedKmh;
            var durationMinutes = (int)Math.Ceiling(durationHours * 60);

            return durationMinutes;
        }

        private decimal GetSurgeMultiplier()
        {
            // In production, this would be dynamic based on:
            // - Current demand
            // - Available drivers
            // - Time of day
            // - Special events

            var hour = DateTime.UtcNow.Hour;

            // Peak hours surge pricing
            if ((hour >= 7 && hour <= 10) || (hour >= 17 && hour <= 20))
            {
                return 1.2m; // 20% surge during peak hours
            }

            return 1.0m; // No surge
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
