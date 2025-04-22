using Grand.Domain.Shipping;

namespace Shipping.DHL.Interfaces
{
    public interface IDhlDeliveryService : DHL24WebapiPort
    {

        AuthData ProvideCredentials();

    }
}
