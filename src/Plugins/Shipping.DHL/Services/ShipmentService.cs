using Microsoft.Extensions.Options;
using Shipping.DHL.Common;

namespace Shipping.DHL.Services
{
    public class ShipmentService
    {
        private readonly AuthData _authData;

        public ShipmentService(IOptions<DhlAuth> authOptions)
        {
            _authData = new AuthData {
                username = authOptions.Value.UsernameField,
                password = authOptions.Value.PasswordField
            };
        }


        public createShipmentsRequest CreateShipmentsRequest()
        {

            // Prepare shipment(s)
            var shipment = new ShipmentFullData {
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
        },
            };

            return new createShipmentsRequest {
                authData = _authData,
                shipments = new[] { shipment }
            };
        }
    }
}
