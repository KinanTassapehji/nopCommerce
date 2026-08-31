using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Menus;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Topics;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Menus;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using NopStation.Plugin.Widgets.MegaMenu.Domains;
using NopStation.Plugin.Widgets.MegaMenu.Infrastructure.Cache;
using NopStation.Plugin.Widgets.MegaMenu.Models;
using NopStation.Plugin.Widgets.MegaMenu.Services;

namespace NopStation.Plugin.Widgets.MegaMenu.Factories;

public class MegaMenuModelFactory : IMegaMenuModelFactory
{
	private readonly BlogSettings _blogSettings;

	private readonly ICategoryIconService _categoryIconService;

	private readonly CatalogSettings _catalogSettings;

	private readonly DisplayDefaultMenuItemSettings _displayDefaultMenuItemSettings;

	private readonly ForumSettings _forumSettings;

	private readonly ICategoryService _categoryService;

	private readonly ILocalizationService _localizationService;

	private readonly IPictureService _pictureService;

	private readonly IProductService _productService;

	private readonly IStaticCacheManager _cacheManager;

	private readonly IStoreContext _storeContext;

	private readonly ITopicService _topicService;

	private readonly IUrlRecordService _urlRecordService;

	private readonly IWebHelper _webHelper;

	private readonly IWorkContext _workContext;

	private readonly MediaSettings _mediaSettings;

	private readonly MegaMenuSettings _megaMenuSettings;

	private readonly IMegaMenuCoreService _megaMenuCoreService;

	private readonly ICustomerService _customerService;

	private readonly IMenuService _menuService;

	public MegaMenuModelFactory(BlogSettings blogSettings, ICategoryIconService categoryIconService, CatalogSettings catalogSettings, DisplayDefaultMenuItemSettings displayDefaultMenuItemSettings, ForumSettings forumSettings, ICategoryService categoryService, ILocalizationService localizationService, IPictureService pictureService, IProductService productService, IStaticCacheManager cacheManager, IStoreContext storeContext, ITopicService topicService, IUrlRecordService urlRecordService, IWebHelper webHelper, IWorkContext workContext, MediaSettings mediaSettings, MegaMenuSettings megaMenuSettings, IMegaMenuCoreService megaMenuCoreService, ICustomerService customerService, IMenuService menuService)
	{
		_blogSettings = blogSettings;
		_categoryIconService = categoryIconService;
		_catalogSettings = catalogSettings;
		_displayDefaultMenuItemSettings = displayDefaultMenuItemSettings;
		_forumSettings = forumSettings;
		_categoryService = categoryService;
		_localizationService = localizationService;
		_pictureService = pictureService;
		_productService = productService;
		_cacheManager = cacheManager;
		_storeContext = storeContext;
		_topicService = topicService;
		_urlRecordService = urlRecordService;
		_webHelper = webHelper;
		_workContext = workContext;
		_mediaSettings = mediaSettings;
		_megaMenuSettings = megaMenuSettings;
		_megaMenuCoreService = megaMenuCoreService;
		_customerService = customerService;
		_menuService = menuService;
	}

	/// <remarks>
	/// ponytail: 4.90 dropped Topic/Category.IncludeInTopMenu - main-menu membership now lives in the Menus tables.
	/// </remarks>
	protected virtual async Task<HashSet<int>> GetMainMenuEntityIdsAsync(MenuItemType menuItemType)
	{
		var store = await _storeContext.GetCurrentStoreAsync();
		var menu = (await _menuService.GetAllMenusAsync(MenuType.Main, store.Id)).FirstOrDefault();
		if (menu == null)
			return new HashSet<int>();

		var items = await _menuService.GetAllMenuItemsAsync(menu.Id, storeId: store.Id);
		return items.Where(i => i.MenuItemTypeId == (int)menuItemType && i.EntityId.HasValue)
			.Select(i => i.EntityId.Value).ToHashSet();
	}

	protected virtual async Task<List<MegaMenuModel.TopicModel>> PrepareTopicMenuModelsAsync()
	{
		IStaticCacheManager cacheManager = _cacheManager;
		CacheKey mEGAMENU_TOPICS_MODEL_KEY = MegaMenuModelCacheEventConsumer.MEGAMENU_TOPICS_MODEL_KEY;
		object obj = await _workContext.GetWorkingLanguageAsync();
		ICustomerService customerService = _customerService;
		object obj2 = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
		Store store = await _storeContext.GetCurrentStoreAsync();
		CacheKey key = cacheManager.PrepareKeyForDefaultCache(mEGAMENU_TOPICS_MODEL_KEY, obj, obj2, store);
		return await (await _cacheManager.GetAsync(key, async delegate
		{
			ITopicService topicService = _topicService;
			HashSet<int> menuTopicIds = await GetMainMenuEntityIdsAsync(MenuItemType.TopicPage);
			return (await topicService.GetAllTopicsAsync((await _storeContext.GetCurrentStoreAsync()).Id)).Where((Topic t) => menuTopicIds.Contains(t.Id)).SelectAwait<Topic, MegaMenuModel.TopicModel>(async delegate(Topic t)
			{
				MegaMenuModel.TopicModel topicModel = new MegaMenuModel.TopicModel
				{
					Id = t.Id
				};
				MegaMenuModel.TopicModel topicModel2 = topicModel;
				topicModel2.Name = await _localizationService.GetLocalizedAsync(t, (Topic x) => x.Title);
				MegaMenuModel.TopicModel topicModel3 = topicModel;
				topicModel3.SeName = await _urlRecordService.GetSeNameAsync(t);
				return topicModel;
			}).ToListAsync();
		}));
	}

	protected virtual async Task<List<CategoryMenuModel>> PrepareCategoryMenuModelsAsync()
	{
		bool loadPicture = _megaMenuSettings.ShowCategoryPicture;
		bool loadRightPanelPicture = _megaMenuSettings.ShowMainCategoryPictureRight;
		IStaticCacheManager cacheManager = _cacheManager;
		CacheKey mEGAMENU_CATEGORIES_MODEL_KEY = MegaMenuModelCacheEventConsumer.MEGAMENU_CATEGORIES_MODEL_KEY;
		object obj = await _workContext.GetWorkingLanguageAsync();
		ICustomerService customerService = _customerService;
		object obj2 = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
		Store store = await _storeContext.GetCurrentStoreAsync();
		CacheKey key = cacheManager.PrepareKeyForDefaultCache(mEGAMENU_CATEGORIES_MODEL_KEY, obj, obj2, store);
		return await _cacheManager.GetAsync(key, async delegate
		{
			List<int> ids = new List<int>();
			if (!string.IsNullOrWhiteSpace(_megaMenuSettings.SelectedCategoryIds))
			{
				ids = _megaMenuSettings.SelectedCategoryIds.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
			}
			IMegaMenuCoreService megaMenuCoreService = _megaMenuCoreService;
			IList<Category> allCategories = await megaMenuCoreService.GetCategoriesByIdsAsync((await _storeContext.GetCurrentStoreAsync()).Id, ids);
			return await PrepareCategoryMenuModelsAsync(allCategories, 0, loadPicture, loadRightPanelPicture, loadSubCategories: true, skipItems: true);
		});
	}

	protected virtual async Task<List<CategoryMenuModel>> PrepareCategoryMenuModelsAsync(IList<Category> allCategories, int rootCategoryId, bool loadPicture = true, bool loadRightPanelPicture = true, bool loadSubCategories = true, bool skipItems = false)
	{
		List<CategoryMenuModel> result = new List<CategoryMenuModel>();
		int pictureSize = _mediaSettings.CategoryThumbPictureSize;
		List<Category> list = allCategories.Where((Category c) => c.ParentCategoryId == rootCategoryId).ToList();
		HashSet<int> menuCategoryIds = await GetMainMenuEntityIdsAsync(MenuItemType.Category);
		foreach (Category category in list)
		{
			CategoryMenuModel categoryMenuModel = new CategoryMenuModel
			{
				Id = category.Id
			};
			CategoryMenuModel categoryMenuModel2 = categoryMenuModel;
			categoryMenuModel2.Name = await _localizationService.GetLocalizedAsync(category, (Category x) => x.Name);
			CategoryMenuModel categoryMenuModel3 = categoryMenuModel;
			categoryMenuModel3.SeName = await _urlRecordService.GetSeNameAsync(category);
			categoryMenuModel.IncludeInTopMenu = menuCategoryIds.Contains(category.Id);
			CategoryMenuModel categoryModel = categoryMenuModel;
			if (loadPicture)
			{
				int categoryIconPictureId = 0;
				CategoryIcon categoryIcon = await _categoryIconService.GetCategoryIconByCategoryIdAsync(category.Id);
				if (categoryIcon != null)
				{
					categoryIconPictureId = categoryIcon.PictureId;
				}
				else
				{
					categoryIconPictureId = _megaMenuSettings.DefaultCategoryIconId;
				}
				IStaticCacheManager cacheManager = _cacheManager;
				CacheKey categoryPictureModelKey = NopModelCacheDefaults.CategoryPictureModelKey;
				object obj = categoryIconPictureId;
				object obj2 = pictureSize;
				object obj3 = true;
				object obj4 = await _workContext.GetWorkingLanguageAsync();
				object obj5 = _webHelper.IsCurrentConnectionSecured();
				Store store = await _storeContext.GetCurrentStoreAsync();
				CacheKey key = cacheManager.PrepareKeyForDefaultCache(categoryPictureModelKey, obj, obj2, obj3, obj4, obj5, store);
				categoryMenuModel = categoryModel;
				categoryMenuModel.PictureModel = await _cacheManager.GetAsync(key, async delegate
				{
					Picture picture = await _pictureService.GetPictureByIdAsync(categoryIconPictureId);
					if (picture == null)
					{
						picture = await _pictureService.GetPictureByIdAsync(_megaMenuSettings.DefaultCategoryIconId);
					}
					PictureModel pictureModel = new PictureModel();
					PictureModel pictureModel2 = pictureModel;
					pictureModel2.FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Item1;
					PictureModel pictureModel3 = pictureModel;
					pictureModel3.ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, pictureSize)).Item1;
					PictureModel pictureModel4 = pictureModel;
					pictureModel4.Title = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat"), categoryModel.Name);
					PictureModel pictureModel5 = pictureModel;
					pictureModel5.AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat"), categoryModel.Name);
					return pictureModel;
				});
			}
			if (loadRightPanelPicture)
			{
				IStaticCacheManager cacheManager = _cacheManager;
				CacheKey categoryPictureModelKey = NopModelCacheDefaults.CategoryPictureModelKey;
				object obj5 = category.PictureId;
				object obj4 = pictureSize;
				object obj3 = true;
				object obj2 = await _workContext.GetWorkingLanguageAsync();
				object obj = _webHelper.IsCurrentConnectionSecured();
				Store store = await _storeContext.GetCurrentStoreAsync();
				CacheKey key2 = cacheManager.PrepareKeyForDefaultCache(categoryPictureModelKey, obj5, obj4, obj3, obj2, obj, store);
				categoryMenuModel = categoryModel;
				categoryMenuModel.ParentPicture = await _cacheManager.GetAsync(key2, async delegate
				{
					Task<Picture> picture = _pictureService.GetPictureByIdAsync(category.PictureId);
					PictureModel pictureModel = new PictureModel();
					PictureModel pictureModel2 = pictureModel;
					pictureModel2.FullSizeImageUrl = await _pictureService.GetPictureUrlAsync(picture?.Id ?? 0);
					PictureModel pictureModel3 = pictureModel;
					pictureModel3.ImageUrl = await _pictureService.GetPictureUrlAsync(picture?.Id ?? 0, pictureSize);
					PictureModel pictureModel4 = pictureModel;
					pictureModel4.Title = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat"), categoryModel.Name);
					PictureModel pictureModel5 = pictureModel;
					pictureModel5.AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat"), categoryModel.Name);
					return pictureModel;
				});
			}
			if (_megaMenuSettings.ShowNumberOfCategoryProducts)
			{
				IStaticCacheManager cacheManager = _cacheManager;
				CacheKey categoryPictureModelKey = NopModelCacheDefaults.ProductAttributePictureModelKey;
				ICustomerService customerService = _customerService;
				object obj = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
				Store store = await _storeContext.GetCurrentStoreAsync();
				CacheKey key3 = cacheManager.PrepareKeyForDefaultCache(categoryPictureModelKey, obj, store, category);
				categoryMenuModel = categoryModel;
				categoryMenuModel.NumberOfProducts = await _cacheManager.GetAsync(key3, async delegate
				{
					List<int> categoryIds = new List<int> { category.Id };
					if (_megaMenuSettings.ShowNumberOfCategoryProductsIncludeSubcategories)
					{
						List<int> list2 = categoryIds;
						ICategoryService categoryService = _categoryService;
						int id = category.Id;
						list2.AddRange(await categoryService.GetChildCategoryIdsAsync(id, (await _storeContext.GetCurrentStoreAsync()).Id));
					}
					IProductService productService = _productService;
					IList<int> categoryIds2 = categoryIds;
					return await productService.GetNumberOfProductsInCategoryAsync(categoryIds2, (await _storeContext.GetCurrentStoreAsync()).Id);
				});
			}
			if (loadSubCategories)
			{
				List<CategoryMenuModel> collection = await PrepareCategoryMenuModelsAsync(allCategories, category.Id, _megaMenuSettings.ShowSubcategoryPicture, loadSubCategories);
				categoryModel.SubCategories.AddRange(collection);
			}
			result.Add(categoryModel);
		}
		return result;
	}

	protected virtual async Task<List<ManufacturerMenuModel>> PrepareManufactureMenuModelsAsync(List<int> selectedManufactureIds)
	{
		int pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
		IStaticCacheManager cacheManager = _cacheManager;
		CacheKey mEGAMENU_MANUFACTURERS_MODEL_KEY = MegaMenuModelCacheEventConsumer.MEGAMENU_MANUFACTURERS_MODEL_KEY;
		object obj = await _workContext.GetWorkingLanguageAsync();
		ICustomerService customerService = _customerService;
		object obj2 = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
		Store store = await _storeContext.GetCurrentStoreAsync();
		CacheKey key = cacheManager.PrepareKeyForDefaultCache(mEGAMENU_MANUFACTURERS_MODEL_KEY, obj, obj2, store);
		return await (await _cacheManager.GetAsync(key, async delegate
		{
			IMegaMenuCoreService megaMenuCoreService = _megaMenuCoreService;
			return (await megaMenuCoreService.GetManufacturersByIdsAsync((await _storeContext.GetCurrentStoreAsync()).Id, selectedManufactureIds)).SelectAwait<Manufacturer, ManufacturerMenuModel>(async delegate(Manufacturer manufacturer)
			{
				ManufacturerMenuModel manufacturerMenuModel = new ManufacturerMenuModel
				{
					Id = manufacturer.Id
				};
				ManufacturerMenuModel manufacturerMenuModel2 = manufacturerMenuModel;
				manufacturerMenuModel2.Name = await _localizationService.GetLocalizedAsync(manufacturer, (Manufacturer x) => x.Name);
				ManufacturerMenuModel manufacturerMenuModel3 = manufacturerMenuModel;
				manufacturerMenuModel3.SeName = await _urlRecordService.GetSeNameAsync(manufacturer);
				ManufacturerMenuModel model = manufacturerMenuModel;
				if (_megaMenuSettings.ShowManufacturerPicture)
				{
					IStaticCacheManager cacheManager2 = _cacheManager;
					CacheKey manufacturerPictureModelKey = NopModelCacheDefaults.ManufacturerPictureModelKey;
					object obj3 = manufacturer;
					object obj4 = pictureSize;
					object obj5 = true;
					object obj6 = await _workContext.GetWorkingLanguageAsync();
					object obj7 = _webHelper.IsCurrentConnectionSecured();
					Store store2 = await _storeContext.GetCurrentStoreAsync();
					CacheKey key2 = cacheManager2.PrepareKeyForDefaultCache(manufacturerPictureModelKey, obj3, obj4, obj5, obj6, obj7, store2);
					manufacturerMenuModel = model;
					manufacturerMenuModel.PictureModel = await _cacheManager.GetAsync(key2, async delegate
					{
						Picture picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
						PictureModel pictureModel = new PictureModel();
						PictureModel pictureModel2 = pictureModel;
						pictureModel2.FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Item1;
						PictureModel pictureModel3 = pictureModel;
						pictureModel3.ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, pictureSize)).Item1;
						PictureModel pictureModel4 = pictureModel;
						pictureModel4.Title = string.Format(await _localizationService.GetResourceAsync("Media.Manufacturer.ImageLinkTitleFormat"), model.Name);
						PictureModel pictureModel5 = pictureModel;
						pictureModel5.AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Manufacturer.ImageAlternateTextFormat"), model.Name);
						return pictureModel;
					});
				}
				return model;
			}).ToListAsync();
		}));
	}

	public virtual async Task<MegaMenuModel> PrepareMegaMenuModelAsync()
	{
		IStaticCacheManager cacheManager = _cacheManager;
		CacheKey mEGAMENU_MODEL_KEY = MegaMenuModelCacheEventConsumer.MEGAMENU_MODEL_KEY;
		object obj = await _workContext.GetWorkingLanguageAsync();
		ICustomerService customerService = _customerService;
		object obj2 = await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
		Store store = await _storeContext.GetCurrentStoreAsync();
		CacheKey key = cacheManager.PrepareKeyForDefaultCache(mEGAMENU_MODEL_KEY, obj, obj2, store);
		return await _cacheManager.GetAsync(key, async delegate
		{
			MegaMenuModel model = new MegaMenuModel
			{
				NewProductsEnabled = _catalogSettings.NewProductsEnabled,
				BlogEnabled = _blogSettings.Enabled,
				ForumEnabled = _forumSettings.ForumsEnabled,
				DisplayHomePageMenuItem = _displayDefaultMenuItemSettings.DisplayHomepageMenuItem,
				DisplayNewProductsMenuItem = _displayDefaultMenuItemSettings.DisplayNewProductsMenuItem,
				DisplayProductSearchMenuItem = _displayDefaultMenuItemSettings.DisplayProductSearchMenuItem,
				DisplayCustomerInfoMenuItem = _displayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem,
				DisplayBlogMenuItem = _displayDefaultMenuItemSettings.DisplayBlogMenuItem,
				DisplayForumsMenuItem = _displayDefaultMenuItemSettings.DisplayForumsMenuItem,
				DisplayContactUsMenuItem = _displayDefaultMenuItemSettings.DisplayContactUsMenuItem,
				MaxCategoryLevelsToShow = _megaMenuSettings.MaxCategoryLevelsToShow,
				HideManufacturers = _megaMenuSettings.HideManufacturers
			};
			MegaMenuModel megaMenuModel = model;
			megaMenuModel.Topics = await PrepareTopicMenuModelsAsync();
			megaMenuModel = model;
			megaMenuModel.Categories = await PrepareCategoryMenuModelsAsync();
			if (!_megaMenuSettings.HideManufacturers)
			{
				List<int> selectedManufactureIds = new List<int>();
				if (!string.IsNullOrWhiteSpace(_megaMenuSettings.SelectedManufacturerIds))
				{
					selectedManufactureIds = _megaMenuSettings.SelectedManufacturerIds.Split(',').Select(int.Parse).ToList();
				}
				megaMenuModel = model;
				megaMenuModel.Manufacturers = await PrepareManufactureMenuModelsAsync(selectedManufactureIds);
			}
			return model;
		});
	}
}
