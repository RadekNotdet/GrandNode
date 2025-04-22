using Grand.Domain.Shipping;
using MediatR;
using Shipping.DHL.Interfaces;

namespace Shipping.DHL.CQRS.Queries.GetDeliveryPrice
{
    public class GetDeliveryPriceQueryHandler : IRequestHandler<GetDeliveryPriceQuery, GetDeliveryPriceResponse>
    {
        private readonly IMediator _mediator;
        private readonly IDhlDeliveryService _dhlShipmentService;
        private readonly DHL24WebapiPortClient _client;


        public GetDeliveryPriceQueryHandler(IMediator mediator, DHL24WebapiPortClient client)
        {
            _mediator = mediator;
            _client = client;
        }

        public async Task<GetDeliveryPriceResponse> Handle(GetDeliveryPriceQuery query, CancellationToken cancellationToken)
        {
            var pricePayment = new PricePayment { };
            var shipperAddress = new ShipperAddress { };
            var receiverAddress = new ReceiverAddress { };
            var priceService = new PriceService();
            var pricePieces = new PricePiece[] { };


            var request = new GetPriceRequest {
                payment = pricePayment,
                shipper = shipperAddress,
                receiver = receiverAddress,
                pieceList = pricePieces,
                service = priceService
            };
            return await _mediator.Send(new GetDeliveryPriceQuery { });
        }
    }
}
