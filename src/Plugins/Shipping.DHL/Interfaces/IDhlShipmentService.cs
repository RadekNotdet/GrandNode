using Grand.Domain.Shipping;

namespace Shipping.DHL.Interfaces
{
    public interface IDhlShipmentService
    {

        AuthData ProvideAuthorization();
        createShipmentsRequest CreateShipmentsRequest(Shipment[] appShipmentsReceiver);

    }
}
