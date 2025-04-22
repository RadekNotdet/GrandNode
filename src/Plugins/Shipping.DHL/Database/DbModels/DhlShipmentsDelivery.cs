using Grand.Domain;

namespace Shipping.DHL.Database.DbModels
{
    public class DhlShipmentsDelivery : BaseEntity
    {
        public string DhlShipmentId { get; set; } //or public string ExternalProviderShipmentId { get; set; }
        public string PickupOrderId  { get; set; }

        // public string ExternalProviderName { get; set; }
        public string Status { get; set; }

    }
}
