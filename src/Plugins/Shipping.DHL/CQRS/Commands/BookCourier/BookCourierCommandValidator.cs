using FluentValidation;

namespace Shipping.DHL.CQRS.Commands.BookCourier
{
    public class BookCourierCommandValidator : AbstractValidator<BookCourierCommand>
    {
        public BookCourierCommandValidator()
        {
            RuleFor(x => x.PickupDate)
                .NotEmpty().WithMessage("Pickup date is required")
                .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Pickup date must be in YYYY-MM-DD format");

            RuleFor(x => x.PickupTimeFrom)
                .NotEmpty().WithMessage("Pickup time from is required")
                .Matches(@"^\d{2}:\d{2}$").WithMessage("Time format must be HH:MM");

            RuleFor(x => x.PickupTimeTo)
                .NotEmpty().WithMessage("Pickup time to is required")
                .Matches(@"^\d{2}:\d{2}$").WithMessage("Time format must be HH:MM");

            RuleFor(x => x.ShipmentsIdList)
                .NotNull().WithMessage("Shipment list cannot be null")
                .Must(list => list.Any()).WithMessage("At least one shipment ID must be provided");

            RuleFor(x => x.AdditionalInfo)
                .MaximumLength(50).WithMessage("Additional info cannot exceed 50 characters");
        }
    }
}
