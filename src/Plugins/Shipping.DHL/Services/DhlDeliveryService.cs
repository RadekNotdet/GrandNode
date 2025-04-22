using Azure.Core;
using Azure.Messaging.EventGrid.SystemEvents;
using Grand.Domain.Shipping;
using Microsoft.Extensions.Options;
using Shipping.DHL.Common;
using Shipping.DHL.Interfaces;

namespace Shipping.DHL.Services
{
    public class DhlDeliveryService : DHL24WebapiPortClient ,IDhlDeliveryService
    {
        private readonly AuthData _authData;
        private readonly DHL24WebapiPort _client;

        public DhlDeliveryService(IOptions<DhlAuth> authOptions, DHL24WebapiPort client)
        {
            _authData = new AuthData {
                username = authOptions.Value.UsernameField,
                password = authOptions.Value.PasswordField
            };

            _client = client;
        }


        public AuthData ProvideCredentials()
        {
            return _authData;
        }
       
    }
}
