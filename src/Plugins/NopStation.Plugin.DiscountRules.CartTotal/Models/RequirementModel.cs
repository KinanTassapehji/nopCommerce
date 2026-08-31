using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.DiscountRules.CartTotal.Models;

public class RequirementModel
{
	public int DiscountId { get; set; }

	public int RequirementId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.DiscountRules.CartTotal.Fields.MinimumCartTotal")]
	public decimal MinimumCartTotal { get; set; }
}
