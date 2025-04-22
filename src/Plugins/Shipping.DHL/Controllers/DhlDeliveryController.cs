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
using Shipping.DHL.Controllers.Models;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Data;
using Shipping.DHL.Database.DbModels;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using MongoDB.Driver.Linq;

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
        private readonly IOrderService _orderService;
        private readonly IRepository<DhlShipmentsDelivery> _dhlDeliveryRepository;

        public DhlDeliveryController(IMediator mediator, IContextAccessor
            contextAccessor, IGroupService groupService, IShipmentService shipmentService, IOrderService orderService, IRepository<DhlShipmentsDelivery> dhlDeliveryRepository)
        {
            _mediator = mediator;
            _contextAccessor = contextAccessor;
            _groupService = groupService;
            _shipmentService = shipmentService;
            _orderService = orderService;
            _dhlDeliveryRepository = dhlDeliveryRepository;
        }

        [HttpPost]
        public async Task<IActionResult> BookCourier(BookCourierModel model)
        {
            var shipments = new List<Shipment>();
            var shipments_access = new List<Shipment>();
            if (model.selectedIds != null) shipments.AddRange(await _shipmentService.GetShipmentsByIds(model.selectedIds.ToArray()));

            var storeId = "";
            if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer))
                storeId = _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId;

            //??? 
            shipments_access = shipments.Where(x => x.StoreId == storeId || string.IsNullOrEmpty(storeId)).ToList();

            foreach (var shipment in shipments_access)
            {
                var order = await _orderService.GetOrderById(shipment.OrderId);

                if (order.ShippingMethod.ToUpper().Contains("DHL") 
                    && order.ShippingStatusId==ShippingStatus.Pending)
                {
                    try
                    {
                        var bookCourierResult = await _mediator.Send(new BookCourierCommand {
                            PickupDate = model.PickupDate,
                            PickupTimeFrom = model.PickupTimeFrom,
                            PickupTimeTo = model.PickupTimeTo,
                            AdditionalInfo = model.Additionalinfo,
                            CourierWithLabel = true,
                            ShipmentsIdList = new[] { shipment.ExternalDeliveryShipmentId }
                        });

                        if(string.IsNullOrEmpty(bookCourierResult))
                        order.ShippingStatusId = ShippingStatus.AwaitingCourier;
                        await _orderService.UpdateOrder(order);

                        
                        string bookCourierOrderId = bookCourierResult;
                        string dhlInternalShipmentId = shipment.ExternalDeliveryShipmentId;

                        //get externaldeliveryprovider shipmentid (from provider's db set)
                        var dhlDeliveryEntity = await _dhlDeliveryRepository.Table
                            .Where(column =>column.DhlShipmentId.ToUpper().Contains(shipment.ExternalDeliveryShipmentId.ToUpper()))
                            .FirstAsync();
                        dhlDeliveryEntity.PickupOrderId = bookCourierOrderId;
                        await _dhlDeliveryRepository.UpdateAsync(dhlDeliveryEntity);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, $"{ex.Message}");
                    }
                } 
            }
            return Ok();
        }

        [HttpPost("create-dhl-shipments")]
        public async Task<IActionResult> CreateDelivery([FromBody] Shipment[] shipments)
        {
          throw new NotImplementedException();
        }

        [HttpPost("create-dhl-shipments")]
        public async Task<IActionResult> GetDeliveries(string createdFrom, string createdTo)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("delete-dhl-shipments")]
        public async Task<IActionResult> DeleteDelivery(ICollection<string> dhlShpimentsIds)
        {
            throw new NotImplementedException();
        }

        [HttpGet("get-postal-code-services")]
        public async Task<IActionResult> GetPostalCodeServices(string postCode, string pickupDate)
        {
            throw new NotImplementedException();
        }


        [HttpPost("get-dhl-price")]
        public Task<IActionResult> GetDeliveryPrice(object TODO)
        {
            throw new NotImplementedException();
        }

        [HttpPost("get-dhl-labels")]
        public async Task<IActionResult> GetLabelsForDelivery([FromBody] ItemToPrint[] labelsToPrint)
        {
          throw new NotImplementedException();
        }

      

        [HttpPost("get-dhl-shipment-status")]
        public async Task<IActionResult> GetDeliveryInfo([FromBody] string dhlShipmentId)
        {
            throw new NotImplementedException();
        }




    }
}
