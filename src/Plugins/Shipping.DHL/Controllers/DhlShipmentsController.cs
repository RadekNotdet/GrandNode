using Grand.Domain.Shipping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shipping.DHL.Common;
using Shipping.DHL.Interfaces;
using Shipping.DHL.Models;
using Shipping.DHL.Services;

namespace Shipping.DHL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DhlShipmentsController : ControllerBase
    {
        private readonly DHL24WebapiPortClient _client;
        private readonly IDhlShipmentService _shipmentService;

        public DhlShipmentsController(DHL24WebapiPortClient client, DhlShipmentService shipmentService)
        {
            _client = client;
            _shipmentService = shipmentService;
            

        }


        [HttpPost("create-dhl-shipments")]
        public async Task<IActionResult> CreateShipments([FromBody] Shipment[] dhlShipments)
        {
            try
            {
                var request = _shipmentService.CreateShipmentsRequest(dhlShipments);

                var response = await _client.createShipmentsAsync(request.authData, request.shipments);

                return Ok(response.createShipmentsResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending shipment: {ex.Message}");
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

        [HttpPost("book-dhl-courier")]
        public async Task<IActionResult> BookCourier([FromBody] bookCourierRequest request)
        {
            try
            {
                var authorization = _shipmentService.ProvideAuthorization();

                var response = await _client.bookCourierAsync(
                    authorization, 
                    request.pickupDate, 
                    request.pickupTimeFrom, 
                    request.pickupTimeTo, 
                    request.additionalInfo, 
                    request.shipmentIdList, 
                    request.shipmentOrderInfo, 
                    request.courierWithLabel);

                return Ok(response.bookCourierResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error booking courier: {ex.Message}");
            }
        }

        [HttpPost("get-dhl-shipment-status")]
        public async Task<IActionResult> GetShipmentInfo([FromBody] string dhlShipmentId)
        {
            try
            {
                var authorization = _shipmentService.ProvideAuthorization();

                var response = await _client.getTrackAndTraceInfoAsync(
                    authorization,
                   dhlShipmentId);

                return Ok(response.getTrackAndTraceInfoResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting track and trace info: {ex.Message}");
            }
        }

       

    }
}
