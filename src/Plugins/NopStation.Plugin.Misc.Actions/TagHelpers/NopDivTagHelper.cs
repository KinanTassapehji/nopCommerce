using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Core.TagHelpers;

[HtmlTargetElement("nop-div", Attributes = "asp-for", TagStructure = TagStructure.WithoutEndTag)]
public class NopDivTagHelper : TagHelper
{
	private const string FOR_ATTRIBUTE_NAME = "asp-for";

	private const string DISPLAY_CHECK_ATTRIBUTE_NAME = "asp-check-access";

	private const string DISPLAY_VALUE_ATTRIBUTE_NAME = "asp-value";

	private readonly ILocalizationService _localizationService;

	private readonly IWorkContext _workContext;

	private readonly IPermissionService _permissionService;

	protected IHtmlGenerator Generator { get; set; }

	[HtmlAttributeName("asp-for")]
	public ModelExpression For { get; set; }

	[HtmlAttributeName("asp-check-access")]
	public bool CheckAccess { get; set; }

	[HtmlAttributeName("asp-value")]
	public string Value { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; }

	public NopDivTagHelper(IHtmlGenerator generator, ILocalizationService localizationService, IWorkContext workContext, IPermissionService permissionService)
	{
		Generator = generator;
		_localizationService = localizationService;
		_workContext = workContext;
		_permissionService = permissionService;
	}

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		ArgumentNullException.ThrowIfNull(context, "context");
		ArgumentNullException.ThrowIfNull(output, "output");
		output.TagName = "div";
		output.TagMode = TagMode.StartTagAndEndTag;
		string value = (output.Attributes.ContainsName("class") ? $"{output.Attributes["class"].Value} form-text-row" : "form-text-row");
		output.Attributes.SetAttribute("class", value);
		object value2 = (string.IsNullOrWhiteSpace(Value) ? For.Model : Value);
		bool flag = CheckAccess;
		if (flag)
		{
			flag = !(await _permissionService.AuthorizeAsync("ManageNopStationFeatures"));
		}
		if (flag)
		{
			value2 = "<i>hidden text...</i>";
		}
		output.Content.AppendHtml(value2?.ToString() ?? "");
	}
}
