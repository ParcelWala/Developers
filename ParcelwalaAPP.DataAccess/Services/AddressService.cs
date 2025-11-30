using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parcelwala.DataAccess.Data;
using ParcelwalaAPP.DataAccess.DTOs;
using ParcelwalaAPP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.DataAccess.Services
{
    public interface IAddressService
    {
        Task<(bool Success, string Message, List<AddressResponseDto>? Addresses)> GetCustomerAddressesAsync(int UserId);
        Task<(bool Success, string Message, AddressResponseDto? Address)> AddAddressAsync(int customerId, AddAddressRequest request);
        Task<(bool Success, string Message, AddressResponseDto? Address)> UpdateAddressAsync(int customerId, int addressId, UpdateAddressRequest request);
        Task<(bool Success, string Message)> SetDefaultAddressAsync(int customerId, int addressId);
        Task<(bool Success, string Message)> DeleteAddressAsync(int customerId, int addressId);
    }

    public class AddressService : IAddressService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AddressService> _logger;

        public AddressService(AppDbContext context, ILogger<AddressService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, List<AddressResponseDto>? Addresses)> GetCustomerAddressesAsync(int customerId)
        {
            try
            {
                var addresses = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId && a.IsActive)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.UpdatedAt)
                    .Select(a => new AddressResponseDto
                    {
                        address_id = a.Id.ToString(),
                        address_type = a.AddressType,
                        label = a.Label,
                        address = a.Address,
                        landmark = a.Landmark,
                        latitude = a.Latitude,
                        longitude = a.Longitude,
                        contact_name = a.ContactName,
                        contact_phone = a.ContactPhone,
                        is_default = a.IsDefault
                    })
                    .ToListAsync();

                if (!addresses.Any())
                {
                    return (true, "No addresses found", new List<AddressResponseDto>());
                }

                _logger.LogInformation("Retrieved {Count} addresses for customer {CustomerId}",
                    addresses.Count, customerId);

                return (true, "Addresses retrieved successfully", addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for customer {CustomerId}", customerId);
                return (false, "Failed to retrieve addresses", null);
            }
        }

        public async Task<(bool Success, string Message, AddressResponseDto? Address)> AddAddressAsync(
            int customerId, AddAddressRequest request)
        {
            try
            {
                // If this is set as default, unset all other defaults
                if (request.is_default)
                {
                    var existingDefaults = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == customerId && a.IsDefault && a.IsActive)
                        .ToListAsync();

                    foreach (var addr in existingDefaults)
                    {
                        addr.IsDefault = false;
                        addr.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var newAddress = new CustomerAddress
                {
                    CustomerId = customerId,
                    AddressType = request.address_type,
                    Label = request.label,
                    Address = request.address,
                    Landmark = request.landmark,
                    Latitude = request.latitude,
                    Longitude = request.longitude,
                    ContactName = request.contact_name,
                    ContactPhone = request.contact_phone,
                    IsDefault = request.is_default,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CustomerAddresses.Add(newAddress);
                await _context.SaveChangesAsync();

                var responseDto = new AddressResponseDto
                {
                    address_id = newAddress.Id.ToString(),
                    address_type = newAddress.AddressType,
                    label = newAddress.Label,
                    address = newAddress.Address,
                    landmark = newAddress.Landmark,
                    latitude = newAddress.Latitude,
                    longitude = newAddress.Longitude,
                    contact_name = newAddress.ContactName,
                    contact_phone = newAddress.ContactPhone,
                    is_default = newAddress.IsDefault
                };

                _logger.LogInformation("Address {AddressId} added for customer {CustomerId}",
                    newAddress.Id, customerId);

                return (true, "Address added successfully", responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address for customer {CustomerId}", customerId);
                return (false, "Failed to add address", null);
            }
        }
        public async Task<(bool Success, string Message, AddressResponseDto? Address)> UpdateAddressAsync(
            int customerId, int addressId, UpdateAddressRequest request)
        {
            try
            {
                // Find the address to update
                var address = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId && a.IsActive);

                if (address == null)
                {
                    _logger.LogWarning("Address {AddressId} not found for customer {CustomerId}",
                        addressId, customerId);
                    return (false, "Address not found", null);
                }

                // If setting this as default, unset all other defaults
                if (request.is_default && !address.IsDefault)
                {
                    var existingDefaults = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == customerId && a.IsDefault && a.Id != addressId && a.IsActive)
                        .ToListAsync();

                    foreach (var addr in existingDefaults)
                    {
                        addr.IsDefault = false;
                        addr.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Update address fields
                address.AddressType = request.address_type;
                address.Label = request.label;
                address.Address = request.address;
                address.Landmark = request.landmark;
                address.Latitude = request.latitude;
                address.Longitude = request.longitude;
                address.ContactName = request.contact_name;
                address.ContactPhone = request.contact_phone;
                address.IsDefault = request.is_default;
                address.UpdatedAt = DateTime.UtcNow;

                _context.CustomerAddresses.Update(address);
                await _context.SaveChangesAsync();

                var responseDto = new AddressResponseDto
                {
                    address_id = address.Id.ToString(),
                    address_type = address.AddressType,
                    label = address.Label,
                    address = address.Address,
                    landmark = address.Landmark,
                    latitude = address.Latitude,
                    longitude = address.Longitude,
                    contact_name = address.ContactName,
                    contact_phone = address.ContactPhone,
                    is_default = address.IsDefault
                };

                _logger.LogInformation("Address {AddressId} updated for customer {CustomerId}",
                    addressId, customerId);

                return (true, "Address updated successfully", responseDto);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating address {AddressId} for customer {CustomerId}",
                    addressId, customerId);
                return (false, "Address was modified by another process. Please try again.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for customer {CustomerId}",
                    addressId, customerId);
                return (false, "Failed to update address", null);
            }
        }

        public async Task<(bool Success, string Message)> SetDefaultAddressAsync(int customerId, int addressId)
        {
            try
            {
                var address = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId && a.IsActive);

                if (address == null)
                {
                    return (false, "Address not found");
                }

                // Unset all other defaults
                var otherDefaults = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId && a.IsDefault && a.Id != addressId && a.IsActive)
                    .ToListAsync();

                foreach (var addr in otherDefaults)
                {
                    addr.IsDefault = false;
                    addr.UpdatedAt = DateTime.UtcNow;
                }

                // Set new default
                address.IsDefault = true;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Address {AddressId} set as default for customer {CustomerId}",
                    addressId, customerId);

                return (true, "Default address updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId} for customer {CustomerId}",
                    addressId, customerId);
                return (false, "Failed to update default address");
            }
        }

        public async Task<(bool Success, string Message)> DeleteAddressAsync(int customerId, int addressId)
        {
            try
            {
                var address = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId && a.IsActive);

                if (address == null)
                {
                    return (false, "Address not found");
                }
                if (address.IsDefault)
                {
                    return (false, "Cannot delete default address. Please set another address as default first.");
                }

                // Soft delete
                address.IsActive = false;
                address.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Address {AddressId} deleted for customer {CustomerId}",
                    addressId, customerId);

                return (true, "Address deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for customer {CustomerId}",
                    addressId, customerId);
                return (false, "Failed to delete address");
            }
        }
    }
}
