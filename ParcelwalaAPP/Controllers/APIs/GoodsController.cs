using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.DataAccess.Services;

namespace ParcelwalaAPP.Controllers.APIs
{
    [Route("goods")]
    [ApiController]
    public class GoodsController : ControllerBase
    {
        private readonly IGoodsTypeService _goodsTypeService;
        private readonly ILogger<GoodsController> _logger;

        public GoodsController(
            IGoodsTypeService goodsTypeService,
            ILogger<GoodsController> logger)
        {
            _goodsTypeService = goodsTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all goods types (cached for 30 minutes)
        /// </summary>
        /// <returns>List of all goods types</returns>
        [HttpGet("types")]
        [ResponseCache(Duration = 1800)] // 30 minutes
        [ProducesResponseType(typeof(GoodsTypesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GoodsTypesResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GoodsTypesResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GoodsTypesResponse>> GetGoodsTypes()
        {
            try
            {
                _logger.LogInformation("GetGoodsTypes endpoint called");

                var response = await _goodsTypeService.GetAllGoodsTypesAsync();

                if (!response.Success || response.Data.Count == 0)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGoodsTypes endpoint");

                return StatusCode(500, new GoodsTypesResponse
                {
                    Success = false,
                    Message = "An internal error occurred. Please try again later.",
                    Data = new List<GoodsTypeDto>()
                });
            }
        }

        /// <summary>
        /// Get only active goods types (cached for 30 minutes)
        /// </summary>
        /// <returns>List of active goods types</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("types/active")]
        [ResponseCache(Duration = 1800)] // 30 minutes
        [ProducesResponseType(typeof(GoodsTypesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GoodsTypesResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GoodsTypesResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GoodsTypesResponse>> GetActiveGoodsTypes()
        {
            try
            {
                _logger.LogInformation("GetActiveGoodsTypes endpoint called");

                var response = await _goodsTypeService.GetActiveGoodsTypesAsync();

                if (!response.Success || response.Data.Count == 0)
                {
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActiveGoodsTypes endpoint");

                return StatusCode(500, new GoodsTypesResponse
                {
                    Success = false,
                    Message = "An internal error occurred. Please try again later.",
                    Data = new List<GoodsTypeDto>()
                });
            }
        }
    }
}
