namespace Shipping.DHL.Controllers.Models
{
    public class BookCourierModel
    {
        public ICollection<string> selectedIds { get; set; }
        public string PickupDate { get; set; }
        public string PickupTimeFrom { get; set; }
        public string PickupTimeTo { get; set; }
        public string Additionalinfo { get; set; }

    }
}
