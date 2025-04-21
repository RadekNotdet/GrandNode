using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Shipping;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Domain.Shipping;
using Grand.Infrastructure;
using MediatR;
using Shipping.DHL.Interfaces;

namespace Shipping.DHL.CQRS.Commands.BookCourier
{
    public class BookCourierCommandHandler : IRequestHandler<BookCourierCommand, string>
    {
        private readonly IMediator _mediator;
        private readonly IDhlDeliveryService _dhlDeliveryService;

        public BookCourierCommandHandler(IMediator mediator, IDhlDeliveryService dhlShipmentService)
        {
            _mediator = mediator;
            _dhlDeliveryService = dhlShipmentService;
        }
        public async Task<string> Handle(BookCourierCommand command, CancellationToken cancellationToken)
        {


            var bookingRequest = new bookCourierRequest {
                authData = _dhlDeliveryService.ProvideCredentials(),
                pickupDate = command.PickupDate,
                pickupTimeFrom = command.PickupTimeFrom,
                pickupTimeTo = command.PickupTimeTo,
                shipmentIdList = command.ShipmentsIdList,
                additionalInfo = command.AdditionalInfo,
                courierWithLabel = command.CourierWithLabel
            };

            var bookingResponse = await _dhlDeliveryService.bookCourierAsync(bookingRequest);


            if (string.IsNullOrEmpty(bookingResponse.bookCourierResult[0]))
            {
                throw new Exception($"Failed to order a courier pickup for DHL Delivery: {command.ShipmentsIdList[0].ToString()}.");
            }


            return bookingResponse.bookCourierResult[0];
        }
    }
}

