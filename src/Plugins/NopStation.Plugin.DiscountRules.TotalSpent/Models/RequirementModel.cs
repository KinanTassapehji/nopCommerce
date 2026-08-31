using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.DiscountRules.TotalSpent.Models;

public class RequirementModel
{
	public int DiscountId { get; set; }

	public int RequirementId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.DiscountRules.TotalSpent.Fields.Amount")]
	public decimal Amount { get; set; }
}
