using Shipping.DHL.Models.Enums;

namespace Shipping.DHL.Models
{
    public class DhlServiceType
    {
        public DhlDeliveryServiceCode Code { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }

        public static List<DhlServiceType> GetAll() => new()
        {
        new() {
            Code = DhlDeliveryServiceCode.AH,
            Name = "Przesyłka krajowa",
            Description = "Standardowa przesyłka krajowa DHL."
        },
        new() {
            Code = DhlDeliveryServiceCode.PR,
            Name = "Przesyłka Premium",
            Description = "Priorytetowa przesyłka krajowa z szybszym doręczeniem."
        },
        new() {
            Code = DhlDeliveryServiceCode._09,
            Name = "Domestic 09",
            Description = "Dostawa do godziny 9:00 następnego dnia roboczego."
        },
        new() {
            Code = DhlDeliveryServiceCode._12,
            Name = "Domestic 12",
            Description = "Dostawa do godziny 12:00 następnego dnia roboczego."
        },
        new() {
            Code = DhlDeliveryServiceCode.DW,
            Name = "Evening Delivery",
            Description = "Dostawa w godzinach wieczornych (17:00–22:00)."
        },
        new() {
            Code = DhlDeliveryServiceCode.SP,
            Name = "Dostawa do punktu",
            Description = "Przesyłka dostarczona do punktu DHL POP lub automatu DHL."
        },
        new() {
            Code = DhlDeliveryServiceCode.EK,
            Name = "Connect",
            Description = "Ekonomiczna przesyłka międzynarodowa w UE."
        },
        new() {
            Code = DhlDeliveryServiceCode.PI,
            Name = "International",
            Description = "Międzynarodowa przesyłka kurierska poza UE."
        },
        new() {
            Code = DhlDeliveryServiceCode.CP,
            Name = "Connect Plus",
            Description = "Szybsza przesyłka międzynarodowa w UE."
        },
        new() {
            Code = DhlDeliveryServiceCode.CM,
            Name = "Connect Plus Pallet",
            Description = "Paletowa przesyłka międzynarodowa Connect Plus."
        }
    };
    }
}
