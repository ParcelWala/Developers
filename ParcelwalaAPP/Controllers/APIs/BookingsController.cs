using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.DataAccess.Services;

namespace ParcelwalaAPP.Controllers.APIs
{
    [Route("bookings")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IFareCalculationService _fareService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            IFareCalculationService fareService,
            ILogger<BookingsController> logger)
        {
            _fareService = fareService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate fare for a trip
        /// </summary>
        /// <param name="request">Fare calculation request with pickup/drop coordinates</param>
        /// <returns>Calculated fare with breakdown</returns>
        [HttpPost("calculate-fare")]
        [ProducesResponseType(typeof(CalculateFareResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CalculateFareResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CalculateFareResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CalculateFareResponse>> CalculateFare(
            [FromBody] CalculateFareRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new CalculateFareResponse
                    {
                        success = false,
                        message = "Validation failed",
                        data = null
                    });
                }

                var (success, message, data) = await _fareService.CalculateFareAsync(
                    request.vehicle_type_id,
                    request.pickup_latitude,
                    request.pickup_longitude,
                    request.drop_latitude,
                    request.drop_longitude
                );

                if (!success)
                {
                    return BadRequest(new CalculateFareResponse
                    {
                        success = false,
                        message = message,
                        data = null
                    });
                }

                return Ok(new CalculateFareResponse
                {
                    success = true,
                    message = message,
                    data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CalculateFare endpoint");

                return StatusCode(500, new CalculateFareResponse
                {
                    success = false,
                    message = "An internal error occurred. Please try again later.",
                    data = null
                });
            }
        }
    }
}
