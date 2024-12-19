using System.ComponentModel.DataAnnotations.Schema;

namespace HieuEMart.Models
{
	public class BillingAddressModel
	{
		public int Id { get; set; }

		[ForeignKey("OrderModel")]
		public int OrderId { get; set; }
		public OrderModel Order { get; set; }
		public string FullName { get; set; }
		public string PhoneNumber { get; set; }
		public string SpecificAddress { get; set; }
		public string Ward { get; set; }
		public string District { get; set; }
		public string Province { get; set; }
	}
}
