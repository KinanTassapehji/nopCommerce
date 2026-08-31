using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Core.TagHelpers;

[HtmlTargetElement("nop-editor-check", Attributes = "asp-for", TagStructure = TagStructure.WithoutEndTag)]
public class NopEditorTagHelper : TagHelper
{
	private const string FOR_ATTRIBUTE_NAME = "asp-for";

	private const string DISABLED_ATTRIBUTE_NAME = "asp-disabled";

	private const string REQUIRED_ATTRIBUTE_NAME = "asp-required";

	private const string RENDER_FORM_CONTROL_CLASS_ATTRIBUTE_NAME = "asp-render-form-control-class";

	private const string TEMPLATE_ATTRIBUTE_NAME = "asp-template";

	private const string POSTFIX_ATTRIBUTE_NAME = "asp-postfix";

	private const string VALUE_ATTRIBUTE_NAME = "asp-value";

	private const string PLACEHOLDER_ATTRIBUTE_NAME = "placeholder";

	private const string AUTOCOMPLETE_ATTRIBUTE_NAME = "autocomplete";

	private readonly IHtmlHelper _htmlHelper;

	private readonly IPermissionService _permissionService;

	[HtmlAttributeName("asp-for")]
	public ModelExpression For { get; set; }

	[HtmlAttributeName("asp-disabled")]
	public string IsDisabled { get; set; }

	[HtmlAttributeName("asp-required")]
	public string IsRequired { get; set; }

	[HtmlAttributeName("placeholder")]
	public string Placeholder { get; set; }

	[HtmlAttributeName("autocomplete")]
	public string Autocomplete { get; set; }

	[HtmlAttributeName("asp-render-form-control-class")]
	public string RenderFormControlClass { get; set; }

	[HtmlAttributeName("asp-template")]
	public string Template { get; set; }

	[HtmlAttributeName("asp-postfix")]
	public string Postfix { get; set; }

	[HtmlAttributeName("asp-value")]
	public string Value { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; }

	public NopEditorTagHelper(IHtmlHelper htmlHelper, IPermissionService permissionService)
	{
		_htmlHelper = htmlHelper;
		_permissionService = permissionService;
	}

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		ArgumentNullException.ThrowIfNull(context, "context");
		ArgumentNullException.ThrowIfNull(output, "output");
		output.SuppressOutput();
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (!string.IsNullOrEmpty(Placeholder))
		{
			dictionary.Add("placeholder", Placeholder);
		}
		if (!string.IsNullOrEmpty(Autocomplete))
		{
			dictionary.Add("autocomplete", Autocomplete);
		}
		if (!string.IsNullOrEmpty(Value))
		{
			dictionary.Add("value", Value);
		}
		if (bool.TryParse(IsDisabled, out var result) & result)
		{
			dictionary.Add("disabled", "disabled");
		}
		if (bool.TryParse(IsRequired, out var result2) & result2)
		{
			output.PreElement.SetHtmlContent("<div class='input-group input-group-required'>");
			output.PostElement.SetHtmlContent("<div class=\"input-group-btn\"><span class=\"required\">*</span></div></div>");
		}
		(_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);
		bool.TryParse(RenderFormControlClass, out var result3);
		if ((string.IsNullOrEmpty(RenderFormControlClass) && For.Metadata.ModelType.Name.Equals("String")) | result3)
		{
			dictionary.Add("class", "form-control");
		}
		string pattern = "Locales(?=\\[\\w+\\]\\.)";
		if (!_htmlHelper.ViewData.ContainsKey(For.Name) && Regex.IsMatch(For.Name, pattern))
		{
			_htmlHelper.ViewData.Add(For.Name, For.Model);
		}
		IHtmlContent htmlOutput = _htmlHelper.Editor(For.Name, Template, new
		{
			htmlAttributes = dictionary,
			postfix = Postfix
		});
		bool flag = For.Metadata.ModelType.Name.Equals("String");
		if (flag)
		{
			flag = !(await _permissionService.AuthorizeAsync("ManageNopStationFeatures"));
		}
		if (flag)
		{
			htmlOutput = new HtmlString($"<input class=\"form-control text-box single-line valid\" data-val=\"true\" id=\"{For.Name}\" id=\"{For.Name}\" type=\"text\" value=\"***********\" aria-describedby=\"{For.Name}-error\" aria-invalid=\"false\">");
		}
		output.Content.SetHtmlContent(htmlOutput);
	}
}
