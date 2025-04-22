using Grand.Domain.Shipping;
using MediatR;
namespace Shipping.DHL.CQRS.Commands.BookCourier
{
    public class BookCourierCommand : IRequest<string>
    {
        public string PickupDate { get; set; } // RRRR-MM-DD
        public string PickupTimeFrom { get; set; } //HH:MM
        public string PickupTimeTo { get; set; } //HH:MM
        public string[] ShipmentsIdList { get; set; } //Wewnętrzne shipmenty DHL
        public string? AdditionalInfo { get; set; } 
        public bool? CourierWithLabel { get; set; }

    }
}
