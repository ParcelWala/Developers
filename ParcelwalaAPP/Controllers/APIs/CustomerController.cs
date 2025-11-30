using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcelwala.DataAccess.Data;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.DataAccess.Services;
using ParcelwalaAPP.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ParcelwalaAPP.Controllers.APIs
{
    [Route("customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly AppDbContext _context;
        private readonly IAddressService _addressService;
      

        public CustomerController(ILogger<CustomerController> logger, 
            AppDbContext context, IAddressService addressService)
        {
            _logger = logger;
            _context = context;          
            _addressService = addressService;
        }     

       

        //----------------------------Addresss & Services----------------------------

        /// <summary>
        /// Get all saved addresses for authenticated customer
        /// </summary>
        [Authorize]
        [HttpGet("addresses")]
        [ProducesResponseType(typeof(GetAddressesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GetAddressesResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GetAddressesResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetAddressesResponse>> GetAddresses()
        {
            try
            {
                // Get customer ID from JWT token
                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int userId))
                {
                    return Unauthorized(new GetAddressesResponse
                    {
                        success = false,
                        message = "Invalid or missing authentication token"
                    });
                }
                // Get customer
                var customer = await _context.CustomerProfiles
                    .FirstOrDefaultAsync(u => u.UserID == userId);
                if (customer == null)
                {                  
                    return StatusCode(500, new GetAddressesResponse
                    {
                        success = false,
                        message = "Customer not found"
                    });
                }
                else
                {
                    var customerId = customer.CustomerID;
                    var (success, message, addresses) = await _addressService.GetCustomerAddressesAsync(customerId);

                    if (!success)
                    {
                        return StatusCode(500, new GetAddressesResponse
                        {
                            success = false,
                            message = message
                        });
                    }

                    return Ok(new GetAddressesResponse
                    {
                        success = true,
                        message = message,
                        Data = addresses ?? new List<AddressResponseDto>()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAddresses endpoint");

                return StatusCode(500, new GetAddressesResponse
                {
                    success = false,
                    message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Add a new address for authenticated customer
        /// </summary>
       
        [Authorize]
        [HttpPost("addresses")]
        [ProducesResponseType(typeof(GetAddressesResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(GetAddressesResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetAddressesResponse>> AddAddress([FromBody] AddAddressRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new GetAddressesResponse
                    {
                        success = false,
                        message = "Validation failed",
                        Data = new List<AddressResponseDto>()
                    });
                }

                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int userId))
                {
                    return Unauthorized(new GetAddressesResponse
                    {
                        success = false,
                        message = "Invalid authentication token"
                    });
                }
                // Get customer
                var customer = await _context.CustomerProfiles
                    .FirstOrDefaultAsync(u => u.UserID == userId);
                if (customer == null)
                {
                    return StatusCode(500, new GetAddressesResponse
                    {
                        success = false,
                        message = "Customer not found"
                    });
                }
                else
                {
                    var customerId = customer.CustomerID;
                    var (success, message, address) = await _addressService.AddAddressAsync(customerId, request);

                    if (!success)
                    {
                        return BadRequest(new GetAddressesResponse
                        {
                            success = false,
                            message = message
                        });
                    }

                    return CreatedAtAction(nameof(GetAddresses), new GetAddressesResponse
                    {
                        success = true,
                        message = message,
                        Data = address != null ? new List<AddressResponseDto> { address } : new List<AddressResponseDto>()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAddress endpoint");

                return StatusCode(500, new GetAddressesResponse
                {
                    success = false,
                    message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Update an existing address for authenticated customer
        /// </summary>
        [HttpPut("addresses/{addressId}")]
        [ProducesResponseType(typeof(UpdateAddressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateAddressResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UpdateAddressResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(UpdateAddressResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UpdateAddressResponse>> UpdateAddress(
            int addressId,
            [FromBody] UpdateAddressRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new UpdateAddressResponse
                    {
                        success = false,
                        message = "Validation failed"
                    });
                }

                // Get customer ID from JWT token
                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int userId))
                {
                    return Unauthorized(new UpdateAddressResponse
                    {
                        success = false,
                        message = "Invalid or missing authentication token"
                    });
                }
                // Get customer
                var customer = await _context.CustomerProfiles
                    .FirstOrDefaultAsync(u => u.UserID == userId);
                if (customer == null)
                {
                    return StatusCode(500, new GetAddressesResponse
                    {
                        success = false,
                        message = "Customer not found"
                    });
                }
                else
                {
                    var customerId = customer.CustomerID;

                    var (success, message, address) = await _addressService.UpdateAddressAsync(
                    customerId, addressId, request);

                    if (!success)
                    {
                        if (message.Contains("not found"))
                        {
                            return NotFound(new UpdateAddressResponse
                            {
                                success = false,
                                message = message
                            });
                        }

                        return BadRequest(new UpdateAddressResponse
                        {
                            success = false,
                            message = message
                        });
                    }

                    return Ok(new UpdateAddressResponse
                    {
                        success = true,
                        message = message,
                        Data = address
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAddress endpoint for addressId: {AddressId}", addressId);

                return StatusCode(500, new UpdateAddressResponse
                {
                    success = false,
                    message = "An internal error occurred"
                });
            }
        }


        [HttpDelete("addresses/{addressId}")]
        [ProducesResponseType(typeof(GetAddressesResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(GetAddressesResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetAddressesResponse>> DeleteAddress(int addressId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new GetAddressesResponse
                    {
                        success = false,
                        message = "Validation failed",
                        Data = new List<AddressResponseDto>()
                    });
                }

                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerIdClaim) || !int.TryParse(customerIdClaim, out int userId))
                {
                    return Unauthorized(new GetAddressesResponse
                    {
                        success = false,
                        message = "Invalid authentication token"
                    });
                }
                // Get customer
                var customer = await _context.CustomerProfiles
                    .FirstOrDefaultAsync(u => u.UserID == userId);
                if (customer == null)
                {
                    return StatusCode(500, new GetAddressesResponse
                    {
                        success = false,
                        message = "Customer not found"
                    });
                }
                else
                {
                    var customerId = customer.CustomerID;
                    var (success, message) = await _addressService.DeleteAddressAsync(customerId, addressId);

                    if (!success)
                    {
                        return BadRequest(new GetAddressesResponse
                        {
                            success = false,
                            message = message
                        });
                    }

                    return CreatedAtAction(nameof(GetAddresses), new GetAddressesResponse
                    {
                        success = true,
                        message = message,
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAddress endpoint");

                return StatusCode(500, new GetAddressesResponse
                {
                    success = false,
                    message = "An internal error occurred"
                });
            }
        }


    }






}
