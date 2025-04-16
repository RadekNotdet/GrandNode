using Grand.Domain.Shipping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shipping.DHL.Common;
using Shipping.DHL.Models;
using Shipping.DHL.Services;

namespace Shipping.DHL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DhlShipmentsController : ControllerBase
    {
        private readonly DHL24WebapiPortClient _client;
        private readonly ShipmentService _shipmentService;

        public DhlShipmentsController(DHL24WebapiPortClient client, ShipmentService shipmentService)
        {
            _client = client;
            _shipmentService = shipmentService;
            

        }


        [HttpPost("create-dhl-shipments")]
        public async Task<IActionResult> CreateShipments([FromBody] Shipment[] shipments)
        {
            try
            {
                var request = _shipmentService.CreateShipmentsRequest(shipments);

                var response = await _client.createShipmentsAsync(request.authData, request.shipments);

                return Ok(response.createShipmentsResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending shipment: {ex.Message}");
            }
        }

        [HttpPost("book-dhl-courier")]
        public async Task<IActionResult> BookCourier([FromBody] DhlCourierPickupRequest request)
        {
            try
            {
                var authorization = _shipmentService.ProvideAuthorization();

                var response = await _client.bookCourierAsync(
                    authorization, 
                    request.PickupDate, 
                    request.PickupTimeFrom, 
                    request.PickupTimeTo, 
                    request.AdditionalInfo, 
                    request.ShipmentIdList, 
                    request.ShipmentOrderInfo, 
                    request.CourierWithLabel);

                return Ok(response.bookCourierResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error booking courier: {ex.Message}");
            }
        }

        [HttpPost("get-dhl-labels")]
        public async Task<IActionResult> GetLabels([FromBody] ItemToPrint[] request)
        {
            try
            {
                var authorization = _shipmentService.ProvideAuthorization();

                var response = await _client.getLabelsAsync(
                    authorization,
                   request);

                return Ok(response.getLabelsResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting labels: {ex.Message}");
            }
        }

    }
}
