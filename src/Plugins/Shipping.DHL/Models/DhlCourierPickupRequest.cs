using System.Text.Json.Serialization;

namespace Shipping.DHL.Models
{
    public class DhlCourierPickupRequest
    {

            [JsonPropertyName("pickupDate")]
            public string PickupDate { get; set; } // Format: YYYY-MM-DD

            [JsonPropertyName("pickupTimeFrom")]
            public string PickupTimeFrom { get; set; } // Format: HH:MM

            [JsonPropertyName("pickupTimeTo")]
            public string PickupTimeTo { get; set; } // Format: HH:MM

            [JsonPropertyName("additionalInfo")]
            public string? AdditionalInfo { get; set; } // Optional, max 50 characters

            [JsonPropertyName("shipmentIdList")]
            public string[] ShipmentIdList { get; set; }

            [JsonPropertyName("shipmentOrderInfo")]
            public ShipmentOrderInfo ShipmentOrderInfo { get; set; }

            [JsonPropertyName("courierWithLabel")]
            public bool? CourierWithLabel { get; set; } // Optional
    }
}
