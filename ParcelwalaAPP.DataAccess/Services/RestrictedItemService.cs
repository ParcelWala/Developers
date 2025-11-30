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
    public interface IRestrictedItemService
    {
        Task<RestrictedItemsResponse> GetAllRestrictedItemsAsync();
        Task<RestrictedItemsResponse> GetRestrictedItemsByCategoryAsync(string category);
    }

    public class RestrictedItemService : IRestrictedItemService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RestrictedItemService> _logger;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "RestrictedItems_All";
        private const string CategoryCacheKeyPrefix = "RestrictedItems_Category_";
        private readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        public RestrictedItemService(
            AppDbContext context,
            ILogger<RestrictedItemService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<RestrictedItemsResponse> GetAllRestrictedItemsAsync()
        {
            try
            {
                // Try to get from cache first
                if (_cache.TryGetValue(CacheKey, out List<RestrictedItemDto>? cachedItems)
                    && cachedItems != null)
                {
                    _logger.LogInformation("Retrieved {Count} restricted items from cache", cachedItems.Count);

                    return new RestrictedItemsResponse
                    {
                        Success = true,
                        Message = "Restricted items retrieved successfully",
                        Data = cachedItems
                    };
                }

                // If not in cache, fetch from database
                var restrictedItems = await _context.RestrictedItems
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.ItemId)
                    .Select(r => new RestrictedItemDto
                    {
                        ItemId = r.ItemId,
                        Name = r.Name,
                        Description = r.Description,
                        Category = r.Category
                    })
                    .ToListAsync();

                if (restrictedItems.Count == 0)
                {
                    _logger.LogWarning("No restricted items found in database");

                    return new RestrictedItemsResponse
                    {
                        Success = false,
                        Message = "No restricted items found",
                        Data = new List<RestrictedItemDto>()
                    };
                }

                // Cache the results for 24 hours
                _cache.Set(CacheKey, restrictedItems, CacheDuration);

                _logger.LogInformation("Retrieved and cached {Count} restricted items from database",
                    restrictedItems.Count);

                return new RestrictedItemsResponse
                {
                    Success = true,
                    Message = "Restricted items retrieved successfully",
                    Data = restrictedItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving restricted items");

                return new RestrictedItemsResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving restricted items",
                    Data = new List<RestrictedItemDto>()
                };
            }
        }

        public async Task<RestrictedItemsResponse> GetRestrictedItemsByCategoryAsync(string category)
        {
            try
            {
                var categoryCacheKey = $"{CategoryCacheKeyPrefix}{category}";

                // Try to get from cache first
                if (_cache.TryGetValue(categoryCacheKey, out List<RestrictedItemDto>? cachedItems)
                    && cachedItems != null)
                {
                    _logger.LogInformation("Retrieved {Count} restricted items for category {Category} from cache",
                        cachedItems.Count, category);

                    return new RestrictedItemsResponse
                    {
                        Success = true,
                        Message = $"Restricted items in category '{category}' retrieved successfully",
                        Data = cachedItems
                    };
                }

                // If not in cache, fetch from database
                var restrictedItems = await _context.RestrictedItems
                    .Where(r => r.IsActive && r.Category.ToLower() == category.ToLower())
                    .OrderBy(r => r.ItemId)
                    .Select(r => new RestrictedItemDto
                    {
                        ItemId = r.ItemId,
                        Name = r.Name,
                        Description = r.Description,
                        Category = r.Category
                    })
                    .ToListAsync();

                if (restrictedItems.Count == 0)
                {
                    _logger.LogWarning("No restricted items found for category: {Category}", category);

                    return new RestrictedItemsResponse
                    {
                        Success = false,
                        Message = $"No restricted items found in category '{category}'",
                        Data = new List<RestrictedItemDto>()
                    };
                }

                // Cache the results for 24 hours
                _cache.Set(categoryCacheKey, restrictedItems, CacheDuration);

                _logger.LogInformation("Retrieved and cached {Count} restricted items for category {Category} from database",
                    restrictedItems.Count, category);

                return new RestrictedItemsResponse
                {
                    Success = true,
                    Message = $"Restricted items in category '{category}' retrieved successfully",
                    Data = restrictedItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving restricted items for category: {Category}", category);

                return new RestrictedItemsResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving restricted items",
                    Data = new List<RestrictedItemDto>()
                };
            }
        }
    }
}
