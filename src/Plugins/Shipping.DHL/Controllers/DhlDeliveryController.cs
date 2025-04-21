using Grand.Domain.Shipping;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shipping.DHL.CQRS.Commands.BookCourier;
using Shipping.DHL.CQRS.Commands.CreateDelivery;
using Shipping.DHL.Common;
using Shipping.DHL.CQRS.Commands.BookCourier;
using Shipping.DHL.CQRS.Commands.CreateDelivery;
using Shipping.DHL.Interfaces;
using Shipping.DHL.Models;
using Shipping.DHL.Services;
using Grand.Business.Core.Interfaces.Checkout.Shipping;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Pdf;
using Grand.Infrastructure;

namespace Shipping.DHL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DhlDeliveryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IContextAccessor _contextAccessor;
        private readonly IGroupService _groupService;
        private readonly IShipmentService _shipmentService;

        public DhlDeliveryController(IMediator mediator, IContextAccessor contextAccessor, IGroupService groupService, IShipmentService shipmentService)
        {
            _mediator = mediator;
            _contextAccessor = contextAccessor;
            _groupService = groupService;
            _shipmentService = shipmentService;
        }

        [HttpPost]
        public async Task<IActionResult> BookCourier(ICollection<string> selectedIds)
        {
            var shipments = new List<Shipment>();
            var shipments_access = new List<Shipment>();
            if (selectedIds != null) shipments.AddRange(await _shipmentService.GetShipmentsByIds(selectedIds.ToArray()));

            var storeId = "";
            if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer))
                storeId = _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId;

            shipments_access = shipments.Where(x => x.StoreId == storeId || string.IsNullOrEmpty(storeId)).ToList();

            foreach (var shipment in shipments_access)
            {
                try
                {
                    var bookCourierResult = await _mediator.Send(new BookCourierCommand {
                        PickupDate = "BookCourierForm.PickupDate",
                        PickupTimeFrom = "BookCourierForm.PickupTimeFrom",
                        PickupTimeTo = "BookCourierForm.PickupTimeTo",
                        AdditionalInfo = "BookCourierForm.Additionalinfo",
                        CourierWithLabel = true,
                        ShipmentsIdList = new[] { shipment.ExternalDeliveryShipmentId } });

                    string bookCourierOrderId = bookCourierResult;
                    string dhlInternalShipmentId = shipment.ExternalDeliveryShipmentId;
                    //Update DHL DB with AWAITINGCOURIERSTATUS + PICKUPDELIVERYORDERID / FK - ExternalDeliveryShipmentId
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"DHL API Error: {ex.Message}");
                }
               
                
            }
            return Ok();
        }

        [HttpPost("create-dhl-shipments")]
        public async Task<IActionResult> CreateDelivery([FromBody] Shipment[] shipments)
        {
          throw new NotImplementedException();
        }


        [HttpPost("get-dhl-labels")]
        public async Task<IActionResult> GetLabels([FromBody] ItemToPrint[] labelsToPrint)
        {
          throw new NotImplementedException();
        }

      

        [HttpPost("get-dhl-shipment-status")]
        public async Task<IActionResult> GetDeliveryInfo([FromBody] string dhlShipmentId)
        {
            throw new NotImplementedException();
        }

        [HttpPost("get-dhl-price")]
        public Task<IActionResult> GetDeliveryPrice([FromBody] string dhlShipmentId)
        {
            throw new NotImplementedException();
        }



    }
}
