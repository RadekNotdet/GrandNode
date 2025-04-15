using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shipping.DHL.Common;
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


        [HttpPost("create-shipments")]
        public async Task<IActionResult> CreateShipments()
        {
            try
            {
                var request = _shipmentService.CreateShipmentsRequest();

                var response = await _client.createShipmentsAsync(request.authData, request.shipments);

                return Ok(response.createShipmentsResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending shipment: {ex.Message}");
            }
        }

        [HttpPost("create-shipment")]
        public async Task<IActionResult> CreateShipment()
        {
            try
            {
                var request = _shipmentService.CreateShipmentsRequest();

                var response = await _client.createShipmentsAsync(request.authData, request.shipments);

                return Ok(response.createShipmentsResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending shipment: {ex.Message}");
            }
        }

    }
}
