using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Common;

/// <summary>
/// Represents a welcome slide list model
/// </summary>
public partial record WelcomeSlideListModel : BasePagedListModel<WelcomeSlideModel>;