using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.Core;
using NopStation.Plugin.Misc.Core.Domains.Marketplace;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Factories;

public class MarketplaceModelFactory : IMarketplaceModelFactory
{
	private readonly IPluginService _pluginService;

	private readonly ILogger _logger;

	private readonly IMarketplaceService _marketplaceService;

	public MarketplaceModelFactory(IPluginService pluginService, ILogger logger, IMarketplaceService marketplaceService)
	{
		_pluginService = pluginService;
		_logger = logger;
		_marketplaceService = marketplaceService;
	}

	private static ButtonAction ResolveButtonAction(ProductResponse product, bool isInstalled)
	{
		string text = "4.90";
		if (!product.SupportedVersions.Contains<string>(text, StringComparer.OrdinalIgnoreCase))
		{
			return ButtonAction.RequestForUpgrade;
		}
		if (isInstalled)
		{
			return ButtonAction.Installed;
		}
		if (product.SupportedVersions.Contains(text))
		{
			return ButtonAction.Install;
		}
		return ButtonAction.RequestForUpgrade;
	}

	public async Task<MarketplaceModel> PrepareMarketplaceListModelAsync(MarketplaceSearchModel searchModel)
	{
		MarketplaceModel empty = new MarketplaceModel
		{
			PageNumber = searchModel.PageNumber,
			ActiveCategoryId = searchModel.CategoryId,
			ActivePaidFilter = searchModel.PaidFilter,
			ActiveSearchText = searchModel.SearchText,
			ActiveVersionFilter = searchModel.VersionFilter
		};
		try
		{
			ApiResponse apiResponse = await _marketplaceService.GetMarketplaceProductsAsync(searchModel.CategoryId, searchModel.PaidFilter, searchModel.SearchText, searchModel.VersionFilter, searchModel.PageNumber);
			if (apiResponse == null)
			{
				await _logger.ErrorAsync("NopStation Marketplace: API response is null.");
				return empty;
			}
			HashSet<string> installedSystemNames = new HashSet<string>((await _pluginService.GetPluginDescriptorsAsync<IPlugin>()).Select((PluginDescriptor d) => d.SystemName), StringComparer.OrdinalIgnoreCase);
			List<MarketplaceProductModel> list = apiResponse.Products.Select(delegate(ProductResponse p)
			{
				bool isInstalled = !string.IsNullOrWhiteSpace(p.SystemName) && installedSystemNames.Contains(p.SystemName);
				ButtonAction buttonAction = ResolveButtonAction(p, isInstalled);
				return new MarketplaceProductModel
				{
					Id = p.Id,
					Name = p.Name,
					ShortDescription = p.ShortDescription,
					SupportedVersions = p.SupportedVersions,
					PictureUrl = p.PictureUrl,
					Price = p.Price,
					OldPrice = p.OldPrice,
					FormattedPrice = p.FormattedPrice,
					FormattedOldPrice = p.FormattedOldPrice,
					ProductUrl = p.ProductUrl,
					SystemName = p.SystemName,
					IsInstalled = isInstalled,
					ButtonAction = buttonAction
				};
			}).ToList();
			List<MarketplaceCategoryModel> categories = apiResponse.Categories.Select((CategoryResponse c) => new MarketplaceCategoryModel
			{
				Id = c.Id,
				Name = c.Name,
				ProductCount = c.ProductCount
			}).ToList();
			MarketplaceModel marketplaceModel = new MarketplaceModel();
			marketplaceModel.Categories = categories;
			marketplaceModel.Products = list;
			marketplaceModel.TotalCount = apiResponse.TotalCount;
			marketplaceModel.PageNumber = apiResponse.PageNumber;
			marketplaceModel.PageSize = apiResponse.PageSize;
			marketplaceModel.ActiveCategoryId = searchModel.CategoryId;
			marketplaceModel.ActivePaidFilter = searchModel.PaidFilter;
			marketplaceModel.ActiveSearchText = searchModel.SearchText;
			marketplaceModel.ActiveVersionFilter = searchModel.VersionFilter;
			marketplaceModel.MarketplaceLogoUrl = apiResponse.MarketplaceLogoUrl;
			marketplaceModel.LoadPagedList(new PagedList<MarketplaceProductModel>(list, apiResponse.PageNumber - 1, apiResponse.PageSize, apiResponse.TotalCount));
			return marketplaceModel;
		}
		catch (HttpRequestException exception)
		{
			await _logger.ErrorAsync("NopStation Marketplace: failed to fetch product list from API.", exception);
			return empty;
		}
		catch (TaskCanceledException exception2)
		{
			await _logger.ErrorAsync("NopStation Marketplace: API request timed out.", exception2);
			return empty;
		}
		catch (System.Text.Json.JsonException exception3)
		{
			await _logger.ErrorAsync("NopStation Marketplace: failed to parse API response.", exception3);
			return empty;
		}
	}
}
