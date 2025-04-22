using Grand.Infrastructure.Models;
using Shipping.DHL.Models;

namespace Grand.Web.Models.Checkout;

public class CheckoutModel : BaseModel
{
    public bool ShippingRequired { get; set; }
    public CheckoutBillingAddressModel BillingAddress { get; set; }
    public CheckoutShippingAddressModel ShippingAddress { get; set; }

#nullable enable
    public DhlServiceType? DhlServiceType { get; set; }
}