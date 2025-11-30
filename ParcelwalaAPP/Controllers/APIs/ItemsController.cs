using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.DataAccess.Services;

namespace ParcelwalaAPP.Controllers.APIs
{
    [Route("items")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRestrictedItemService _restrictedItemService;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(
            IRestrictedItemService restrictedItemService,
            ILogger<ItemsController> logger)
        {
            _restrictedItemService = restrictedItemService;
            _logger = logger;
        }

        /// <summary>
        /// Get all restricted items (cached for 24 hours)
        /// </summary>
        /// <returns>List of all restricted items</returns>
       
        [HttpGet("restricted")]
        [ResponseCache(Duration = 86400)] // 24 hours
        [ProducesResponseType(typeof(RestrictedItemsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RestrictedItemsResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RestrictedItemsResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestrictedItemsResponse>> GetRestrictedItems()
        {
            try
            {
                _logger.LogInformation("GetRestrictedItems endpoint called");

                var response = await _restrictedItemService.GetAllRestrictedItemsAsync();

                if (!response.Success || response.Data.Count == 0)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRestrictedItems endpoint");

                return StatusCode(500, new RestrictedItemsResponse
                {
                    Success = false,
                    Message = "An internal error occurred. Please try again later.",
                    Data = new List<RestrictedItemDto>()
                });
            }
        }

        /// <summary>
        /// Get restricted items by category (cached for 24 hours)
        /// Categories: Illegal, Dangerous, Prohibited, Restricted
        /// </summary>
        /// <param name="category">Category name</param>
        /// <returns>List of restricted items in the specified category</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("restricted/category/{category}")]
        [ResponseCache(Duration = 86400)] // 24 hours
        [ProducesResponseType(typeof(RestrictedItemsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RestrictedItemsResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RestrictedItemsResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RestrictedItemsResponse>> GetRestrictedItemsByCategory(string category)
        {
            try
            {
                _logger.LogInformation("GetRestrictedItemsByCategory endpoint called with category: {Category}", category);

                var response = await _restrictedItemService.GetRestrictedItemsByCategoryAsync(category);

                if (!response.Success || response.Data.Count == 0)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRestrictedItemsByCategory endpoint for category: {Category}", category);

                return StatusCode(500, new RestrictedItemsResponse
                {
                    Success = false,
                    Message = "An internal error occurred. Please try again later.",
                    Data = new List<RestrictedItemDto>()
                });
            }
        }
    }
}
