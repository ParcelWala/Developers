using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Parcelwala.DataAccess.Data;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.DataAccess.Services;
using System.Security.Claims;

namespace ParcelwalaAPP.Controllers.APIs
{
    [Route("auth/vehicles")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehicleController> _logger;
        private readonly IVehicleService _vehicleService;

        public VehicleController(ILogger<VehicleController> logger, AppDbContext context,IVehicleService vehicleService)
        {
            _logger = logger; 
            _context = context;
            _vehicleService = vehicleService;
        }

        [HttpPost("types")]
        //[Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VehiclesType()
        {
            // Get User ID from JWT token
          
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                         .SelectMany(v => v.Errors)
                         .Select(e => e.ErrorMessage);

                return BadRequest(new VehicleTypesResponse
                {
                    success = false,
                    message = $"Validation failed: {string.Join(", ", errors)}"
                });
            }
            var response = await _vehicleService.GetAllVehicleTypesAsync();

            if (response.success)
            {
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, response);


        }
    }
}
