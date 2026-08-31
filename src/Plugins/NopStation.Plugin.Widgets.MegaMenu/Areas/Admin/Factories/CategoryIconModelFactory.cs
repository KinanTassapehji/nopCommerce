using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.MegaMenu.Domains;
using NopStation.Plugin.Widgets.MegaMenu.Services;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Factories;

public class CategoryIconModelFactory : ICategoryIconModelFactory
{
	private readonly CatalogSettings _catalogSettings;

	private readonly IBaseAdminModelFactory _baseAdminModelFactory;

	private readonly IDateTimeHelper _dateTimeHelper;

	private readonly ILanguageService _languageService;

	private readonly ILocalizationService _localizationService;

	private readonly ICategoryIconService _categoryIconService;

	private readonly ICategoryService _categoryService;

	private readonly IPictureService _pictureService;

	private readonly MegaMenuSettings _megaMenuSettings;

	public CategoryIconModelFactory(CatalogSettings catalogSettings, IBaseAdminModelFactory baseAdminModelFactory, IDateTimeHelper dateTimeHelper, ILanguageService languageService, ILocalizationService localizationService, ICategoryIconService categoryIconService, ICategoryService categoryService, IPictureService pictureService, MegaMenuSettings megaMenuSettings)
	{
		_catalogSettings = catalogSettings;
		_baseAdminModelFactory = baseAdminModelFactory;
		_dateTimeHelper = dateTimeHelper;
		_languageService = languageService;
		_localizationService = localizationService;
		_categoryIconService = categoryIconService;
		_categoryService = categoryService;
		_pictureService = pictureService;
		_megaMenuSettings = megaMenuSettings;
	}

	protected async Task PrepareAvailableCategoriesAsync(IList<SelectListItem> items, bool excludeDefaultItem = false)
	{
		IList<Category> categories = await _categoryService.GetAllCategoriesAsync();
		foreach (Category category in categories)
		{
			IList<SelectListItem> list = items;
			SelectListItem selectListItem = new SelectListItem();
			SelectListItem selectListItem2 = selectListItem;
			selectListItem2.Text = await _categoryService.GetFormattedBreadCrumbAsync(category, categories);
			selectListItem.Value = category.Id.ToString();
			list.Add(selectListItem);
		}
		if (!excludeDefaultItem)
		{
			IList<SelectListItem> list = items;
			SelectListItem selectListItem2 = new SelectListItem();
			SelectListItem selectListItem = selectListItem2;
			selectListItem.Text = await _localizationService.GetResourceAsync("Admin.Common.All");
			selectListItem2.Value = "0";
			list.Insert(0, selectListItem2);
		}
	}

	public virtual async Task<CategoryIconSearchModel> PrepareCategoryIconSearchModelAsync(CategoryIconSearchModel searchModel)
	{
		if (searchModel == null)
		{
			throw new ArgumentNullException("searchModel");
		}
		await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);
		searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();
		searchModel.SetGridPageSize();
		return searchModel;
	}

	public virtual async Task<CategoryIconListModel> PrepareCategoryIconListModelAsync(CategoryIconSearchModel searchModel)
	{
		if (searchModel == null)
		{
			throw new ArgumentNullException("searchModel");
		}
		IPagedList<Category> categories = await _categoryService.GetAllCategoriesAsync(searchModel.SearchCategoryName, searchModel.SearchStoreId, searchModel.Page - 1, searchModel.PageSize, showHidden: true);
		_categoryIconService.GetAllCategoryIconsAsync(searchModel.Page - 1, searchModel.PageSize);
		return await new CategoryIconListModel().PrepareToGridAsync(searchModel, categories, () => categories.SelectAwait<Category, CategoryIconModel>(async (Category category) => await PrepareCategoryIconModelAsync(null, category)));
	}

	public virtual async Task<CategoryIconModel> PrepareCategoryIconModelAsync(CategoryIconModel model, Category category)
	{
		if (category == null)
		{
			throw new ArgumentNullException("category");
		}
		if (model == null)
		{
			model = new CategoryIconModel();
			CategoryIcon categoryIcon = await _categoryIconService.GetCategoryIconByCategoryIdAsync(category.Id);
			Picture picture = null;
			if (categoryIcon != null)
			{
				model = categoryIcon.ToModel<CategoryIconModel>();
				picture = await _pictureService.GetPictureByIdAsync(categoryIcon.PictureId);
			}
			if (picture == null)
			{
				picture = await _pictureService.GetPictureByIdAsync(_megaMenuSettings.DefaultCategoryIconId);
			}
			model.CategoryId = category.Id;
			CategoryIconModel categoryIconModel = model;
			categoryIconModel.CategoryName = await _categoryService.GetFormattedBreadCrumbAsync(category);
			categoryIconModel = model;
			categoryIconModel.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture, 25)).Item1;
		}
		return model;
	}
}
