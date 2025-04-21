using Grand.Domain;

namespace Shipping.DHL.Database.DbModels
{
    public class DhlShipmentsDelivery : BaseEntity
    {
        public string InternalShipmentId { get; set; }
        public string PickupOrderId  { get; set; }

    }
}
