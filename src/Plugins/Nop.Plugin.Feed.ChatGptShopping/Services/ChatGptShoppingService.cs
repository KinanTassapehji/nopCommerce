using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Plugin.Feed.ChatGptShopping.Domain;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Tax;

namespace Nop.Plugin.Feed.ChatGptShopping.Services;

public class ChatGptShoppingService
{
    #region Fields

    private readonly CurrencySettings _currencySettings;
    private readonly ICategoryService _categoryService;
    private readonly ICurrencyService _currencyService;
    private readonly IHtmlFormatter _htmlFormatter;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private readonly IManufacturerService _manufacturerService;
    private readonly INopFileProvider _nopFileProvider;
    private readonly IPictureService _pictureService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IProductService _productService;
    private readonly ISettingService _settingService;
    private readonly IStoreService _storeService;
    private readonly ITaxService _taxService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IVideoService _videoService;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;
    private readonly LinkGenerator _linkGenerator;

    #endregion

    #region Ctor

    public ChatGptShoppingService(
        CurrencySettings currencySettings,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        IHtmlFormatter htmlFormatter,
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService,
        ILogger logger,
        IManufacturerService manufacturerService,
        INopFileProvider nopFileProvider,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IProductService productService,
        ISettingService settingService,
        IStoreService storeService,
        ITaxService taxService,
        IUrlRecordService urlRecordService,
        IVideoService videoService,
        IWebHelper webHelper,
        IWorkContext workContext,
        LinkGenerator linkGenerator
        )
    {
        _categoryService = categoryService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _htmlFormatter = htmlFormatter;
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;
        _logger = logger;
        _manufacturerService = manufacturerService;
        _nopFileProvider = nopFileProvider;
        _pictureService = pictureService;
        _priceCalculationService = priceCalculationService;
        _productService = productService;
        _settingService = settingService;
        _storeService = storeService;
        _taxService = taxService;
        _urlRecordService = urlRecordService;
        _videoService = videoService;
        _webHelper = webHelper;
        _workContext = workContext;
        _linkGenerator = linkGenerator;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Generate a feed
    /// </summary>
    /// <param name="store">Store</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected async Task GenerateFeedAsync(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var pathToFile = $"{ChatGptShoppingDefaults.FilePathDirectory}{store.Id}-{ChatGptShoppingDefaults.FeedFileName}";
        var localFilePath = _nopFileProvider.GetAbsolutePath(pathToFile);

        await using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        await using var gzip = new GZipStream(fileStream, CompressionLevel.Optimal);
        await using var writer = new StreamWriter(gzip, new UTF8Encoding(false));

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        var languageId = (await _workContext.GetWorkingLanguageAsync())?.Id ?? 0;
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var chatGptShoppingSettings = await _settingService.LoadSettingAsync<ChatGptShoppingSettings>(store.Id);

        // currency
        var currency = await _currencyService.GetCurrencyByIdAsync(chatGptShoppingSettings.CurrencyId);
        if (currency == null || !currency.Published)
            currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

        var productPictureSize = chatGptShoppingSettings.ProductPictureSize;

        var pageIndex = 0;
        while (true)
        {
            var products = await _productService.SearchProductsAsync(pageIndex: pageIndex, pageSize: ChatGptShoppingDefaults.PageSize,
                storeId: store.Id,
                showHidden: false);

            if (!products.Any())
                break;

            foreach (var product in products)
            {
                try
                {
                    var dto = await BuildProductAsync(product, languageId, store, currentCustomer, currency, productPictureSize);

                    if (dto == null)
                        continue;

                    await WriteProductAsync(writer, dto, jsonOptions);
                }
                catch (Exception ex)
                {
                    await _logger.WarningAsync($"{ChatGptShoppingDefaults.SystemName} - Unable to export product {product.Id}.", ex);
                }
            }

            pageIndex++;
        }
    }

    protected static async Task WriteProductAsync(StreamWriter writer, ChatGptProductDto dto, JsonSerializerOptions jsonOptions)
    {
        var json = JsonSerializer.Serialize(dto, jsonOptions);
        await writer.WriteLineAsync(json);
    }

    protected async Task<ChatGptProductDto> BuildProductAsync(Product product, int languageId, Store store, Customer customer, Currency currency, int productPictureSize)
    {
        var productDto = new ChatGptProductDto();

        #region OpenAI Flags

        productDto.IsEligibleSearch = true;
        productDto.IsEligibleCheckout = false;
        productDto.IsAdsEligible = true;

        #endregion

        #region Basic Product Data

        productDto.ItemId = product.Sku;
        productDto.Gtin = product.Gtin;
        productDto.Mpn = product.ManufacturerPartNumber;

        var title = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);

        //title should be not longer than 150 characters
        if (title.Length > 150)
            title = title[..150];

        productDto.Title = title;

        var description = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription, languageId);
        if (string.IsNullOrEmpty(description))
            description = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription, languageId);

        productDto.Description = _htmlFormatter.StripTags(_htmlFormatter.ConvertHtmlToPlainText(description, decode: true));

        var productUrl = _linkGenerator.GetPathByName(
                httpContext: _httpContextAccessor.HttpContext,
                endpointName: "ProductDetails",
                values: new { SeName = await _urlRecordService.GetSeNameAsync(product) });

        productUrl = new Uri(new Uri(store.Url), productUrl).AbsoluteUri;

        productDto.Url = productUrl;

        #endregion

        #region Item Information

        var defaultManufacturer = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).FirstOrDefault();

        if (defaultManufacturer != null)
        {
            productDto.Brand = (await _manufacturerService.GetManufacturerByIdAsync(defaultManufacturer.ManufacturerId))?.Name;
        }

        productDto.Condition = "new"; // Assuming all products are new. Adjust as necessary.

        var defaultProductCategory = (await _categoryService
                    .GetProductCategoriesByProductIdAsync(product.Id))
                    .FirstOrDefault();

        if (defaultProductCategory != null)
        {
            var category = await _categoryService.GetFormattedBreadCrumbAsync(
                category: await _categoryService.GetCategoryByIdAsync(defaultProductCategory.CategoryId),
                separator: ">",
                languageId: languageId);

            if (!string.IsNullOrEmpty(category))
            {
                productDto.ProductCategory = category;
            }
        }

        #endregion

        #region Media

        const int maximumPictures = 10;
        var storeLocation = _webHelper.GetStoreLocation();
        var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id, maximumPictures);
        var additionalImageUrls = new List<string>();

        for (var i = 0; i < pictures.Count; i++)
        {
            var picture = pictures[i];
            var imageUrl = await _pictureService.GetPictureUrlAsync(picture.Id, productPictureSize,
                storeLocation: storeLocation);

            if (i == 0)
            {
                //default image
                productDto.ImageUrl = imageUrl;
            }
            else
            {
                //additional image
                additionalImageUrls.Add(imageUrl);
            }
        }
        if (!pictures.Any())
        {
            //no picture? submit a default one
            var imageUrl = await _pictureService.GetDefaultPictureUrlAsync(productPictureSize, storeLocation: storeLocation);
            productDto.ImageUrl = imageUrl;
        }

        productDto.AdditionalImageUrls = string.Join(",", additionalImageUrls);
        productDto.VideoUrl = (await _videoService.GetVideosByProductIdAsync(product.Id)).FirstOrDefault()?.VideoUrl;

        #endregion

        #region Price & Promotions

        var price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.Price, currency);
        //round price now so it matches the product details page
        price = await _priceCalculationService.RoundPriceAsync(price);

        productDto.Price = price.ToString(new CultureInfo("en-US", false).NumberFormat) + " " + currency.CurrencyCode;

        //calculate price for the maximum quantity if we have tier prices, and choose minimal
        var minPossiblePrice = (await _priceCalculationService.GetFinalPriceAsync(product, customer, store, quantity: int.MaxValue)).finalPrice;
        var finalPriceBase = (await _taxService.GetProductPriceAsync(product, minPossiblePrice)).price;
        price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase, currency);

        //round price now so it matches the product details page
        price = await _priceCalculationService.RoundPriceAsync(price);

        productDto.SalePrice = price.ToString(new CultureInfo("en-US", false).NumberFormat) + " " + currency.CurrencyCode;

        #endregion

        #region Availability & Inventory

        var availability = ChatGptShoppingDefaults.ProductAvailabilityInStock; //in stock by default

        if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock
            && product.BackorderMode == BackorderMode.NoBackorders
            && await _productService.GetTotalStockQuantityAsync(product) <= 0)
        {
            availability = ChatGptShoppingDefaults.ProductAvailabilityOutOfStock;
        }

        productDto.Availability = availability;

        if (product.AvailableForPreOrder &&
            (!product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
            product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow))
        {
            productDto.Availability = ChatGptShoppingDefaults.ProductAvailabilityPreOrder;
            productDto.AvailabilityDate = product.PreOrderAvailabilityStartDateTimeUtc.HasValue ? product.PreOrderAvailabilityStartDateTimeUtc.Value : null;
        }

        #endregion

        #region Merchant Info

        productDto.SellerName = store.Name;
        productDto.SellerUrl = store.Url;

        #endregion

        #region Returns

        productDto.ReturnPolicy = await _localizationService.GetResourceAsync("Plugins.Feed.ChatGptShopping.ReturnPolicy");

        #endregion

        #region Reviews and Q&A

        productDto.ReviewCount = product.ApprovedTotalReviews;
        productDto.StarRating = product.ApprovedRatingSum > 0 ? ((double)product.ApprovedRatingSum / product.ApprovedTotalReviews).ToString("F1") : "0.0";

        #endregion

        #region Geo Tagging

        productDto.TargetCountries = ChatGptShoppingDefaults.GeoTargetingCountryCode;
        productDto.StoreCountry = ChatGptShoppingDefaults.GeoTargetingCountryCode;

        #endregion

        return productDto;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Generate a feed
    /// </summary>
    /// <param name="isAutoSync">A value indicating whether to generate a feed for stores with auto-sync enabled only</param>   
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task GenerateChatGptFeedAsync(bool isAutoSync = true)
    {
        try
        {
            var stores = (await _storeService.GetAllStoresAsync()).ToList();
            foreach (var store in stores)
            {
                var chatGptShoppingSettings = await _settingService.LoadSettingAsync<ChatGptShoppingSettings>(store.Id);
                if (isAutoSync && !chatGptShoppingSettings.AutoSyncEnabled)
                    continue;

                await GenerateFeedAsync(store);
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"{ChatGptShoppingDefaults.SystemName} - Error generating ChatGPT Shopping feed", ex);
        }
    }

    #endregion
}
