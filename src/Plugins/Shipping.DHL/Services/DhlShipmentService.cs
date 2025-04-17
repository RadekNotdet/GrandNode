using Grand.Domain.Shipping;
using Microsoft.Extensions.Options;
using Shipping.DHL.Common;
using Shipping.DHL.Interfaces;

namespace Shipping.DHL.Services
{
    public class DhlShipmentService : IDhlShipmentService
    {
        private readonly AuthData _authData;

        public DhlShipmentService(IOptions<DhlAuth> authOptions)
        {
            _authData = new AuthData {
                username = authOptions.Value.UsernameField,
                password = authOptions.Value.PasswordField
            };
        }

        public AuthData ProvideAuthorization()
        {
            return _authData;
        }


        public createShipmentsRequest CreateShipmentsRequest(Shipment[] appShipmentsReceiver)
        {
            List<ShipmentFullData> dhlShipments = new();

            foreach (Shipment shipm in appShipmentsReceiver)
            {
                var dhlShipment = new ShipmentFullData {
                    // Set the required properties here
                    // These are examples, replace with your actual values
                    shipper = new AddressData {
                        name = "Sender Name",
                        postalCode = "00-001",
                        city = "Warsaw",
                        street = "Main Street",
                        houseNumber = "1A",
                        contactPerson = "John Doe",
                        contactPhone = "123456789",
                        contactEmail = "john@example.com"
                    },
                    receiver = new ReceiverAddressData {
                        name = "Receiver Name",
                        postalCode = "00-002",
                        city = "Krakow",
                        street = "Second Street",
                        houseNumber = "2B",
                        contactPerson = "Jane Doe",
                        contactPhone = "987654321",
                        contactEmail = "jane@example.com"
                    },
                    pieceList = new[]
                    { 
                        new PieceDefinition
                            {
                            type = "PACKAGE",
                            quantity = 1,
                            weight = 2.5f,
                            length = 30,
                            width = 20,
                            height = 15 
                            }
                    }};

                dhlShipments.Add(dhlShipment);
            }

            return new createShipmentsRequest {
                authData = _authData,
                shipments = dhlShipments.ToArray() };
        }
    }
}
