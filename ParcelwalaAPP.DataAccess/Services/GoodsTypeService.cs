using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    public interface IGoodsTypeService
    {
        Task<GoodsTypesResponse> GetAllGoodsTypesAsync();
        Task<GoodsTypesResponse> GetActiveGoodsTypesAsync();
    }

    public class GoodsTypeService : IGoodsTypeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GoodsTypeService> _logger;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "GoodsTypes_All";
        private const string ActiveCacheKey = "GoodsTypes_Active";
        private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public GoodsTypeService(
            AppDbContext context,
            ILogger<GoodsTypeService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GoodsTypesResponse> GetAllGoodsTypesAsync()
        {
            try
            {
                // Try to get from cache first
                if (_cache.TryGetValue(CacheKey, out List<GoodsTypeDto>? cachedTypes)
                    && cachedTypes != null)
                {
                    _logger.LogInformation("Retrieved {Count} goods types from cache", cachedTypes.Count);

                    return new GoodsTypesResponse
                    {
                        Success = true,
                        Message = "Goods types retrieved successfully",
                        Data = cachedTypes
                    };
                }

                // If not in cache, fetch from database
                var goodsTypes = await _context.GoodsTypes
                    .OrderBy(g => g.GoodsTypeId)
                    .Select(g => new GoodsTypeDto
                    {
                        GoodsTypeId = g.GoodsTypeId,
                        Name = g.Name,
                        Icon = g.Icon,
                        DefaultWeight = g.DefaultWeight,
                        DefaultPackages = g.DefaultPackages,
                        DefaultValue = g.DefaultValue,
                        IsActive = g.IsActive
                    })
                    .ToListAsync();

                if (goodsTypes.Count == 0)
                {
                    _logger.LogWarning("No goods types found in database");

                    return new GoodsTypesResponse
                    {
                        Success = false,
                        Message = "No goods types found",
                        Data = new List<GoodsTypeDto>()
                    };
                }

                // Cache the results
                _cache.Set(CacheKey, goodsTypes, CacheDuration);

                _logger.LogInformation("Retrieved and cached {Count} goods types from database",
                    goodsTypes.Count);

                return new GoodsTypesResponse
                {
                    Success = true,
                    Message = "Goods types retrieved successfully",
                    Data = goodsTypes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving goods types");

                return new GoodsTypesResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving goods types",
                    Data = new List<GoodsTypeDto>()
                };
            }
        }

        public async Task<GoodsTypesResponse> GetActiveGoodsTypesAsync()
        {
            try
            {
                // Try to get from cache first
                if (_cache.TryGetValue(ActiveCacheKey, out List<GoodsTypeDto>? cachedTypes)
                    && cachedTypes != null)
                {
                    _logger.LogInformation("Retrieved {Count} active goods types from cache",
                        cachedTypes.Count);

                    return new GoodsTypesResponse
                    {
                        Success = true,
                        Message = "Active goods types retrieved successfully",
                        Data = cachedTypes
                    };
                }

                // If not in cache, fetch from database
                var goodsTypes = await _context.GoodsTypes
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.GoodsTypeId)
                    .Select(g => new GoodsTypeDto
                    {
                        GoodsTypeId = g.GoodsTypeId,
                        Name = g.Name,
                        Icon = g.Icon,
                        DefaultWeight = g.DefaultWeight,
                        DefaultPackages = g.DefaultPackages,
                        DefaultValue = g.DefaultValue,
                        IsActive = g.IsActive
                    })
                    .ToListAsync();

                if (goodsTypes.Count == 0)
                {
                    _logger.LogWarning("No active goods types found in database");

                    return new GoodsTypesResponse
                    {
                        Success = false,
                        Message = "No active goods types found",
                        Data = new List<GoodsTypeDto>()
                    };
                }

                // Cache the results
                _cache.Set(ActiveCacheKey, goodsTypes, CacheDuration);

                _logger.LogInformation("Retrieved and cached {Count} active goods types from database",
                    goodsTypes.Count);

                return new GoodsTypesResponse
                {
                    Success = true,
                    Message = "Active goods types retrieved successfully",
                    Data = goodsTypes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active goods types");

                return new GoodsTypesResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving active goods types",
                    Data = new List<GoodsTypeDto>()
                };
            }
        }
    }
}
