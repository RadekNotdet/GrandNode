using Grand.Domain.Shipping;
using MediatR;

namespace Shipping.DHL.CQRS.Commands.CreateDelivery
{
    public class CreateDeliveryCommand : IRequest<CreateDeliveryResponse>
    {
        public Shipment Shipment { get; set; }
    }
}
