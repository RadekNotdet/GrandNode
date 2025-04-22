using Grand.Business.Core.Interfaces.Checkout.Orders;
using MediatR;
using Shipping.DHL.Interfaces;
using Shipping.DHL.CQRS.Queries.GetDeliveryPrice;

namespace Shipping.DHL.CQRS.Commands.CreateDelivery
{
    public class CreateDeliveryCommandHandler : IRequestHandler<CreateDeliveryCommand, CreateDeliveryResponse>
    {
        private readonly IMediator _mediator;
        private readonly IDhlDeliveryService _dhlShipmentService;
        private readonly DHL24WebapiPort _client;
        private readonly IOrderService _orderService;


        public CreateDeliveryCommandHandler(IMediator mediator, DHL24WebapiPortClient client, IOrderService orderService)
        {
            _mediator = mediator;
            _client = client;
            _orderService = orderService;
        }

        public async Task<CreateDeliveryResponse> Handle(CreateDeliveryCommand request, CancellationToken cancellationToken)
        {


            //AuthData authorize = _dhlShipmentService.ProvideCredentials();
            //var response = await _client.createShipmentsAsync(authorize, request.shipments);
            throw new NotImplementedException();
        }
    }
}

