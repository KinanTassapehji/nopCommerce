using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public class SmsTokenProvider : ISmsTokenProvider
{
	private readonly CatalogSettings _catalogSettings;

	private readonly CurrencySettings _currencySettings;

	private readonly IActionContextAccessor _actionContextAccessor;

	private readonly IAttributeFormatter<AddressAttribute, AddressAttributeValue> _addressAttributeFormatter;

	private readonly ICurrencyService _currencyService;

	private readonly IAttributeFormatter<AddressAttribute, AddressAttributeValue> _customerAttributeFormatter;

	private readonly ICustomerService _customerService;

	private readonly IDateTimeHelper _dateTimeHelper;

	private readonly IDownloadService _downloadService;

	private readonly IEventPublisher _eventPublisher;

	private readonly IGenericAttributeService _genericAttributeService;

	private readonly ILanguageService _languageService;

	private readonly ILocalizationService _localizationService;

	private readonly IOrderService _orderService;

	private readonly IPaymentPluginManager _paymentPluginManager;

	private readonly IPaymentService _paymentService;

	private readonly IPriceFormatter _priceFormatter;

	private readonly IStoreContext _storeContext;

	private readonly IStoreService _storeService;

	private readonly IUrlHelperFactory _urlHelperFactory;

	private readonly IUrlRecordService _urlRecordService;

	private readonly IAttributeFormatter<VendorAttribute, VendorAttributeValue> _vendorAttributeFormatter;

	private readonly IWorkContext _workContext;

	private readonly MessageTemplatesSettings _templatesSettings;

	private readonly PaymentSettings _paymentSettings;

	private readonly StoreInformationSettings _storeInformationSettings;

	private readonly TaxSettings _taxSettings;

	private readonly IAddressService _addressService;

	private readonly IStateProvinceService _stateProvinceService;

	private readonly ICountryService _countryService;

	private readonly IProductService _productService;

	private readonly IHtmlFormatter _htmlFormatter;

	private Dictionary<string, IEnumerable<string>> _allowedTokens;

	public const string OTP_TOKENS = "OTP tokens";

	protected Dictionary<string, IEnumerable<string>> AllowedTokens
	{
		get
		{
			if (_allowedTokens != null)
			{
				return _allowedTokens;
			}
			_allowedTokens = new Dictionary<string, IEnumerable<string>>();
			_allowedTokens.Add(TokenGroupNames.StoreTokens, new string[9] { "%Store.Name%", "%Store.URL%", "%Store.CompanyName%", "%Store.CompanyAddress%", "%Store.CompanyPhoneNumber%", "%Store.CompanyVat%", "%Facebook.URL%", "%Twitter.URL%", "%YouTube.URL%" });
			_allowedTokens.Add(TokenGroupNames.CustomerTokens, new string[12]
			{
				"%Customer.Email%", "%Customer.Username%", "%Customer.FullName%", "%Customer.FirstName%", "%Customer.LastName%", "%Customer.VatNumber%", "%Customer.VatNumberStatus%", "%Customer.CustomAttributes%", "%Customer.PasswordRecoveryURL%", "%Customer.AccountActivationURL%",
				"%Customer.EmailRevalidationURL%", "%Wishlist.URLForCustomer%"
			});
			_allowedTokens.Add(TokenGroupNames.OrderTokens, new string[40]
			{
				"%Order.OrderNumber%", "%Order.CustomerFullName%", "%Order.CustomerEmail%", "%Order.BillingFirstName%", "%Order.BillingLastName%", "%Order.BillingPhoneNumber%", "%Order.BillingEmail%", "%Order.BillingFaxNumber%", "%Order.BillingCompany%", "%Order.BillingAddress1%",
				"%Order.BillingAddress2%", "%Order.BillingCity%", "%Order.BillingCounty%", "%Order.BillingStateProvince%", "%Order.BillingZipPostalCode%", "%Order.BillingCountry%", "%Order.BillingCustomAttributes%", "%Order.Shippable%", "%Order.ShippingMethod%", "%Order.ShippingFirstName%",
				"%Order.ShippingLastName%", "%Order.ShippingPhoneNumber%", "%Order.ShippingEmail%", "%Order.ShippingFaxNumber%", "%Order.ShippingCompany%", "%Order.ShippingAddress1%", "%Order.ShippingAddress2%", "%Order.ShippingCity%", "%Order.ShippingCounty%", "%Order.ShippingStateProvince%",
				"%Order.ShippingZipPostalCode%", "%Order.ShippingCountry%", "%Order.ShippingCustomAttributes%", "%Order.PaymentMethod%", "%Order.VatNumber%", "%Order.CustomValues%", "%Order.CreatedOn%", "%Order.OrderURLForCustomer%", "%Order.PickupInStore%", "%Order.OrderId%"
			});
			_allowedTokens.Add(TokenGroupNames.ShipmentTokens, new string[4] { "%Shipment.ShipmentNumber%", "%Shipment.TrackingNumber%", "%Shipment.TrackingNumberURL%", "%Shipment.URLForCustomer%" });
			_allowedTokens.Add(TokenGroupNames.RefundedOrderTokens, new string[1] { "%Order.AmountRefunded%" });
			_allowedTokens.Add(TokenGroupNames.OrderNoteTokens, new string[2] { "%Order.NewNoteText%", "%Order.OrderNoteAttachmentUrl%" });
			_allowedTokens.Add(TokenGroupNames.SubscriptionTokens, new string[3] { "%NewsLetterSubscription.Email%", "%NewsLetterSubscription.ActivationUrl%", "%NewsLetterSubscription.DeactivationUrl%" });
			_allowedTokens.Add(TokenGroupNames.ProductTokens, new string[5] { "%Product.ID%", "%Product.Name%", "%Product.ShortDescription%", "%Product.SKU%", "%Product.StockQuantity%" });
			_allowedTokens.Add(TokenGroupNames.ReturnRequestTokens, new string[9] { "%ReturnRequest.CustomNumber%", "%ReturnRequest.OrderId%", "%ReturnRequest.Product.Quantity%", "%ReturnRequest.Product.Name%", "%ReturnRequest.Reason%", "%ReturnRequest.RequestedAction%", "%ReturnRequest.CustomerComment%", "%ReturnRequest.StaffNotes%", "%ReturnRequest.Status%" });
			_allowedTokens.Add(TokenGroupNames.ForumTokens, new string[2] { "%Forums.ForumURL%", "%Forums.ForumName%" });
			_allowedTokens.Add(TokenGroupNames.ForumTopicTokens, new string[2] { "%Forums.TopicURL%", "%Forums.TopicName%" });
			_allowedTokens.Add(TokenGroupNames.ForumPostTokens, new string[2] { "%Forums.PostAuthor%", "%Forums.PostBody%" });
			_allowedTokens.Add(TokenGroupNames.PrivateMessageTokens, new string[2] { "%PrivateMessage.Subject%", "%PrivateMessage.Text%" });
			_allowedTokens.Add(TokenGroupNames.VendorTokens, new string[3] { "%Vendor.Name%", "%Vendor.Email%", "%Vendor.VendorAttributes%" });
			_allowedTokens.Add(TokenGroupNames.ProductReviewTokens, new string[5] { "%ProductReview.ProductName%", "%ProductReview.Title%", "%ProductReview.IsApproved%", "%ProductReview.ReviewText%", "%ProductReview.ReplyText%" });
			_allowedTokens.Add(TokenGroupNames.AttributeCombinationTokens, new string[3] { "%AttributeCombination.Formatted%", "%AttributeCombination.SKU%", "%AttributeCombination.StockQuantity%" });
			_allowedTokens.Add(TokenGroupNames.VatValidation, new string[2] { "%VatValidationResult.Name%", "%VatValidationResult.Address%" });
			_allowedTokens.Add(TokenGroupNames.ContactVendor, new string[3] { "%ContactUs.SenderEmail%", "%ContactUs.SenderName%", "%ContactUs.Body%" });
			_allowedTokens.Add("OTP tokens", new string[1] { "%Shipment.OTP%" });
			return _allowedTokens;
		}
	}

	public SmsTokenProvider(CatalogSettings catalogSettings, CurrencySettings currencySettings, IActionContextAccessor actionContextAccessor, IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter, ICurrencyService currencyService, IAttributeFormatter<AddressAttribute, AddressAttributeValue> customerAttributeFormatter, ICustomerService customerService, IDateTimeHelper dateTimeHelper, IDownloadService downloadService, IEventPublisher eventPublisher, IGenericAttributeService genericAttributeService, ILanguageService languageService, ILocalizationService localizationService, IOrderService orderService, IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IPriceFormatter priceFormatter, IStoreContext storeContext, IStoreService storeService, IUrlHelperFactory urlHelperFactory, IUrlRecordService urlRecordService, IAttributeFormatter<VendorAttribute, VendorAttributeValue> vendorAttributeFormatter, IWorkContext workContext, MessageTemplatesSettings templatesSettings, PaymentSettings paymentSettings, StoreInformationSettings storeInformationSettings, TaxSettings taxSettings, IAddressService addressService, IStateProvinceService stateProvinceService, ICountryService countryService, IProductService productService, IHtmlFormatter htmlFormatter)
	{
		_catalogSettings = catalogSettings;
		_currencySettings = currencySettings;
		_actionContextAccessor = actionContextAccessor;
		_addressAttributeFormatter = addressAttributeFormatter;
		_currencyService = currencyService;
		_customerAttributeFormatter = customerAttributeFormatter;
		_customerService = customerService;
		_dateTimeHelper = dateTimeHelper;
		_downloadService = downloadService;
		_eventPublisher = eventPublisher;
		_genericAttributeService = genericAttributeService;
		_languageService = languageService;
		_localizationService = localizationService;
		_orderService = orderService;
		_paymentPluginManager = paymentPluginManager;
		_paymentService = paymentService;
		_priceFormatter = priceFormatter;
		_storeContext = storeContext;
		_storeService = storeService;
		_urlHelperFactory = urlHelperFactory;
		_urlRecordService = urlRecordService;
		_vendorAttributeFormatter = vendorAttributeFormatter;
		_workContext = workContext;
		_templatesSettings = templatesSettings;
		_paymentSettings = paymentSettings;
		_storeInformationSettings = storeInformationSettings;
		_taxSettings = taxSettings;
		_addressService = addressService;
		_stateProvinceService = stateProvinceService;
		_countryService = countryService;
		_productService = productService;
		_htmlFormatter = htmlFormatter;
	}

	public IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups)
	{
		return AllowedTokens.Where((KeyValuePair<string, IEnumerable<string>> x) => tokenGroups == null || tokenGroups.Contains(x.Key)).SelectMany((KeyValuePair<string, IEnumerable<string>> x) => x.Value).ToList()
			.Distinct();
	}

	public IEnumerable<string> GetTokenGroups(SmsTemplate smsTemplate)
	{
		switch (smsTemplate.Name)
		{
		case "Customer.EmailValidationMessage":
		case "NewCustomer.Notification":
		case "Customer.WelcomeMessage":
			return new string[2]
			{
				TokenGroupNames.StoreTokens,
				TokenGroupNames.CustomerTokens
			};
		case "OrderPaid.CustomerNotification":
		case "OrderPlaced.VendorNotification":
		case "OrderCancelled.CustomerNotification":
		case "OrderCompleted.CustomerNotification":
		case "OrderPlaced.AdminNotification":
		case "OrderPaid.VendorNotification":
		case "OrderPlaced.CustomerNotification":
			return new string[3]
			{
				TokenGroupNames.StoreTokens,
				TokenGroupNames.OrderTokens,
				TokenGroupNames.CustomerTokens
			};
		case "ShipmentSent.CustomerNotification":
		case "ShipmentDelivered.CustomerNotification":
			return new string[4]
			{
				TokenGroupNames.StoreTokens,
				TokenGroupNames.ShipmentTokens,
				TokenGroupNames.OrderTokens,
				TokenGroupNames.CustomerTokens
			};
		case "ShipmentDelivered.CustomerOTPNotification":
			return new string[5]
			{
				TokenGroupNames.StoreTokens,
				TokenGroupNames.ShipmentTokens,
				TokenGroupNames.OrderTokens,
				TokenGroupNames.CustomerTokens,
				"OTP tokens"
			};
		case "OrderRefunded.AdminNotification":
		case "OrderRefunded.CustomerNotification":
			return new string[4]
			{
				TokenGroupNames.StoreTokens,
				TokenGroupNames.OrderTokens,
				TokenGroupNames.RefundedOrderTokens,
				TokenGroupNames.CustomerTokens
			};
		case "Forums.NewForumTopic":
			return new string[4]
			{
				TokenGroupNames.StoreTokens,
				TokenGroupNames.ForumTopicTokens,
				TokenGroupNames.ForumTokens,
				TokenGroupNames.CustomerTokens
			};
		case "Forums.NewForumPost":
			return new string[5]
			{
				TokenGroupNames.StoreTokens,
				TokenGroupNames.ForumPostTokens,
				TokenGroupNames.ForumTopicTokens,
				TokenGroupNames.ForumTokens,
				TokenGroupNames.CustomerTokens
			};
		case "Customer.NewPM":
			return new string[3]
			{
				TokenGroupNames.StoreTokens,
				TokenGroupNames.PrivateMessageTokens,
				TokenGroupNames.CustomerTokens
			};
		default:
			return new string[0];
		}
	}

	public virtual async Task AddCustomerTokensAsync(IList<Token> tokens, Customer customer)
	{
		tokens.Add(new Token("Customer.Email", customer.Email));
		tokens.Add(new Token("Customer.Username", customer.Username));
		IList<Token> list = tokens;
		list.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
		tokens.Add(new Token("Customer.FirstName", customer.FirstName));
		tokens.Add(new Token("Customer.LastName", customer.LastName));
		tokens.Add(new Token("Customer.VatNumber", customer.VatNumber));
		tokens.Add(new Token("Customer.VatNumberStatus", ((VatNumberStatus)customer.VatNumberStatusId/*cast due to constrained. prefix*/).ToString()));
		string customCustomerAttributesXML = customer.CustomCustomerAttributesXML;
		list = tokens;
		list.Add(new Token("Customer.CustomAttributes", await _customerAttributeFormatter.FormatAttributesAsync(customCustomerAttributesXML), neverHtmlEncoded: true));
		string passwordRecoveryUrl = await RouteUrlAsync(0, "PasswordRecoveryConfirm", new
		{
			token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PasswordRecoveryTokenAttribute),
			email = customer.Email
		});
		string accountActivationUrl = await RouteUrlAsync(0, "AccountActivation", new
		{
			token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.AccountActivationTokenAttribute),
			email = customer.Email
		});
		string emailRevalidationUrl = await RouteUrlAsync(0, "EmailRevalidation", new
		{
			token = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.EmailRevalidationTokenAttribute),
			email = customer.Email
		});
		string value = await RouteUrlAsync(0, "Wishlist", new
		{
			customerGuid = customer.CustomerGuid
		});
		tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, neverHtmlEncoded: true));
		tokens.Add(new Token("Customer.AccountActivationURL", accountActivationUrl, neverHtmlEncoded: true));
		tokens.Add(new Token("Customer.EmailRevalidationURL", emailRevalidationUrl, neverHtmlEncoded: true));
		tokens.Add(new Token("Wishlist.URLForCustomer", value, neverHtmlEncoded: true));
		await _eventPublisher.EntityTokensAddedAsync(customer, tokens);
	}

	public virtual async Task AddStoreTokensAsync(IList<Token> tokens, Store store)
	{
		tokens.Add(new Token("Store.Name", await _localizationService.GetLocalizedAsync(store, (Store x) => x.Name)));
		tokens.Add(new Token("Store.URL", store.Url, neverHtmlEncoded: true));
		tokens.Add(new Token("Store.CompanyName", store.CompanyName));
		tokens.Add(new Token("Store.CompanyAddress", store.CompanyAddress));
		tokens.Add(new Token("Store.CompanyPhoneNumber", store.CompanyPhoneNumber));
		tokens.Add(new Token("Store.CompanyVat", store.CompanyVat));
		tokens.Add(new Token("Facebook.URL", _storeInformationSettings.FacebookLink));
		tokens.Add(new Token("Twitter.URL", _storeInformationSettings.TwitterLink));
		tokens.Add(new Token("YouTube.URL", _storeInformationSettings.YoutubeLink));
		await _eventPublisher.EntityTokensAddedAsync(store, tokens);
	}

	public virtual async Task AddOrderTokensAsync(IList<Token> tokens, Order order, int languageId, int vendorId = 0)
	{
		Address billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
		tokens.Add(new Token("Order.OrderId", order.Id));
		tokens.Add(new Token("Order.OrderNumber", order.CustomOrderNumber));
		tokens.Add(new Token("Order.CustomerFullName", billingAddress.FirstName + " " + billingAddress.LastName));
		tokens.Add(new Token("Order.CustomerEmail", billingAddress.Email));
		tokens.Add(new Token("Order.BillingFirstName", billingAddress.FirstName));
		tokens.Add(new Token("Order.BillingLastName", billingAddress.LastName));
		tokens.Add(new Token("Order.BillingPhoneNumber", billingAddress.PhoneNumber));
		tokens.Add(new Token("Order.BillingEmail", billingAddress.Email));
		tokens.Add(new Token("Order.BillingFaxNumber", billingAddress.FaxNumber));
		tokens.Add(new Token("Order.BillingCompany", billingAddress.Company));
		tokens.Add(new Token("Order.BillingAddress1", billingAddress.Address1));
		tokens.Add(new Token("Order.BillingAddress2", billingAddress.Address2));
		tokens.Add(new Token("Order.BillingCity", billingAddress.City));
		tokens.Add(new Token("Order.BillingCounty", billingAddress.County));
		IList<Token> list = tokens;
		StateProvince stateProvince = await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress);
		string value = ((stateProvince == null) ? string.Empty : (await _localizationService.GetLocalizedAsync(stateProvince, (StateProvince x) => x.Name)));
		list.Add(new Token("Order.BillingStateProvince", value));
		tokens.Add(new Token("Order.BillingZipPostalCode", billingAddress.ZipPostalCode));
		list = tokens;
		Country country = await _countryService.GetCountryByAddressAsync(billingAddress);
		value = ((country == null) ? string.Empty : (await _localizationService.GetLocalizedAsync(country, (Country x) => x.Name)));
		list.Add(new Token("Order.BillingCountry", value));
		list = tokens;
		list.Add(new Token("Order.BillingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync(billingAddress.CustomAttributes), neverHtmlEncoded: true));
		tokens.Add(new Token("Order.Shippable", !string.IsNullOrEmpty(order.ShippingMethod)));
		tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
		tokens.Add(new Token("Order.PickupInStore", order.PickupInStore));
		list = tokens;
		list.Add(new Token("Order.ShippingFirstName", (await orderAddress(order))?.FirstName ?? string.Empty));
		list = tokens;
		list.Add(new Token("Order.ShippingLastName", (await orderAddress(order))?.LastName ?? string.Empty));
		list = tokens;
		list.Add(new Token("Order.ShippingPhoneNumber", (await orderAddress(order))?.PhoneNumber ?? string.Empty));
		list = tokens;
		list.Add(new Token("Order.ShippingEmail", (await orderAddress(order))?.Email ?? string.Empty));
		list = tokens;
		list.Add(new Token("Order.ShippingFaxNumber", (await orderAddress(order))?.FaxNumber ?? string.Empty));
		list = tokens;
		list.Add(new Token("Order.ShippingCompany", (await orderAddress(order))?.Company ?? string.Empty));
		list = tokens;
		list.Add(new Token("Order.ShippingAddress1", (await orderAddress(order))?.Address1 ?? string.Empty));
		list = tokens;
		list.Add(new Token("Order.ShippingAddress2", (await orderAddress(order))?.Address2 ?? string.Empty));
		list = tokens;
		list.Add(new Token("Order.ShippingCity", (await orderAddress(order))?.City ?? string.Empty));
		list = tokens;
		list.Add(new Token("Order.ShippingCounty", (await orderAddress(order))?.County ?? string.Empty));
		list = tokens;
		IStateProvinceService stateProvinceService = _stateProvinceService;
		StateProvince stateProvince2 = await stateProvinceService.GetStateProvinceByAddressAsync(await orderAddress(order));
		value = ((stateProvince2 == null) ? string.Empty : (await _localizationService.GetLocalizedAsync(stateProvince2, (StateProvince x) => x.Name)));
		list.Add(new Token("Order.ShippingStateProvince", value));
		list = tokens;
		list.Add(new Token("Order.ShippingZipPostalCode", (await orderAddress(order))?.ZipPostalCode ?? string.Empty));
		list = tokens;
		ICountryService countryService = _countryService;
		Country country2 = await countryService.GetCountryByAddressAsync(await orderAddress(order));
		value = ((country2 == null) ? string.Empty : (await _localizationService.GetLocalizedAsync(country2, (Country x) => x.Name)));
		list.Add(new Token("Order.ShippingCountry", value));
		list = tokens;
		IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter = _addressAttributeFormatter;
		list.Add(new Token("Order.ShippingCustomAttributes", await addressAttributeFormatter.FormatAttributesAsync((await orderAddress(order))?.CustomAttributes ?? string.Empty), neverHtmlEncoded: true));
		IPaymentMethod paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
		if (paymentMethod != null)
		{
			ILocalizationService localizationService = _localizationService;
			IPaymentMethod plugin = paymentMethod;
			value = await localizationService.GetLocalizedFriendlyNameAsync(plugin, (await _workContext.GetWorkingLanguageAsync()).Id);
		}
		else
		{
			value = order.PaymentMethodSystemName;
		}
		string value2 = value;
		tokens.Add(new Token("Order.PaymentMethod", value2));
		tokens.Add(new Token("Order.VatNumber", order.VatNumber));
		StringBuilder stringBuilder = new StringBuilder();
		CustomValues customValues = new CustomValues();
		customValues.FillByXml(order.CustomValuesXml, displayToCustomerOnly: true);
		if (customValues != null)
		{
			foreach (CustomValue item in customValues)
			{
				stringBuilder.AppendFormat("{0}: {1}", WebUtility.HtmlEncode(item.Name), WebUtility.HtmlEncode((item.Value != null) ? item.Value.ToString() : string.Empty));
				stringBuilder.Append("\\n");
			}
		}
		tokens.Add(new Token("Order.CustomValues", stringBuilder.ToString(), neverHtmlEncoded: true));
		Language language = await _languageService.GetLanguageByIdAsync(languageId);
		if (language != null && !string.IsNullOrEmpty(language.LanguageCulture))
		{
			_customerService.GetCustomerByIdAsync(order.CustomerId);
			tokens.Add(new Token("Order.CreatedOn", (await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc)).ToString("D", new CultureInfo(language.LanguageCulture))));
		}
		else
		{
			tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
		}
		tokens.Add(new Token("Order.OrderURLForCustomer", await RouteUrlAsync(order.StoreId, "OrderDetails", new
		{
			orderId = order.Id
		}), neverHtmlEncoded: true));
		await _eventPublisher.EntityTokensAddedAsync(order, tokens);
		async Task<Address> orderAddress(Order o)
		{
			return await _addressService.GetAddressByIdAsync((o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId).GetValueOrDefault());
		}
	}

	protected virtual async Task<string> RouteUrlAsync(int storeId = 0, string routeName = null, object routeValues = null)
	{
		Store store = await _storeService.GetStoreByIdAsync(storeId);
		if (store == null)
		{
			store = (await _storeContext.GetCurrentStoreAsync()) ?? throw new Exception("No store could be loaded");
		}
		Store store2 = store;
		if (string.IsNullOrEmpty(store2.Url))
		{
			throw new Exception("URL cannot be null");
		}
		IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
		PathString remaining = new PathString(urlHelper.RouteUrl(routeName, routeValues));
		PathString other = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
		remaining.StartsWithSegments(other, out remaining);
		return Uri.EscapeDataString(WebUtility.UrlDecode($"{store2.Url.TrimEnd('/')}{remaining}"));
	}

	public virtual async Task AddShipmentTokensAsync(IList<Token> tokens, Shipment shipment, int languageId)
	{
		tokens.Add(new Token("Shipment.ShipmentNumber", shipment.Id));
		tokens.Add(new Token("Shipment.TrackingNumber", shipment.TrackingNumber));
		string trackingNumberUrl = string.Empty;
		if (!string.IsNullOrEmpty(shipment.TrackingNumber))
		{
			IShipmentTracker shipmentTracker = await NopInstance.Load<IShipmentService>().GetShipmentTrackerAsync(shipment);
			if (shipmentTracker != null)
			{
				trackingNumberUrl = await shipmentTracker.GetUrlAsync(shipment.TrackingNumber);
			}
		}
		tokens.Add(new Token("Shipment.TrackingNumberURL", trackingNumberUrl, neverHtmlEncoded: true));
		tokens.Add(new Token("Shipment.URLForCustomer", await RouteUrlAsync((await _orderService.GetOrderByIdAsync(shipment.OrderId)).StoreId, "ShipmentDetails", new
		{
			shipmentId = shipment.Id
		}), neverHtmlEncoded: true));
		await _eventPublisher.EntityTokensAddedAsync(shipment, tokens);
	}

	public virtual void AddOTPTokens(IList<Token> tokens, string otp)
	{
		tokens.Add(new Token("Shipment.OTP", otp));
	}

	public virtual async Task AddOrderRefundedTokensAsync(IList<Token> tokens, Order order, decimal refundedAmount)
	{
		string currencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
		IPriceFormatter priceFormatter = _priceFormatter;
		string currencyCode2 = currencyCode;
		tokens.Add(new Token("Order.AmountRefunded", await priceFormatter.FormatPriceAsync(refundedAmount, showCurrency: true, currencyCode2, showTax: false, (await _workContext.GetWorkingLanguageAsync()).Id)));
		await _eventPublisher.EntityTokensAddedAsync(order, tokens);
	}

	public virtual async Task AddOrderNoteTokensAsync(IList<Token> tokens, OrderNote orderNote)
	{
		tokens.Add(new Token("Order.NewNoteText", _orderService.FormatOrderNoteText(orderNote), neverHtmlEncoded: true));
		tokens.Add(new Token("Order.OrderNoteAttachmentUrl", await RouteUrlAsync((await _orderService.GetOrderByIdAsync(orderNote.OrderId)).StoreId, "GetOrderNoteFile", new
		{
			ordernoteid = orderNote.Id
		}), neverHtmlEncoded: true));
		await _eventPublisher.EntityTokensAddedAsync(orderNote, tokens);
	}

	public async Task AddNewsLetterSubscriptionTokensAsync(IList<Token> tokens, NewsLetterSubscription subscription)
	{
		tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));
		tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", await RouteUrlAsync(0, "NewsletterActivation", new
		{
			token = subscription.NewsLetterSubscriptionGuid,
			active = "true"
		}), neverHtmlEncoded: true));
		tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", await RouteUrlAsync(0, "NewsletterActivation", new
		{
			token = subscription.NewsLetterSubscriptionGuid,
			active = "false"
		}), neverHtmlEncoded: true));
		await _eventPublisher.EntityTokensAddedAsync(subscription, tokens);
	}

	public async Task AddProductTokensAsync(IList<Token> tokens, Product product, int languageId)
	{
		IProductService productService = NopInstance.Load<IProductService>();
		tokens.Add(new Token("Product.ID", product.Id));
		tokens.Add(new Token("Product.Name", _localizationService.GetLocalizedAsync(product, (Product x) => x.Name, languageId)));
		IList<Token> list = tokens;
		list.Add(new Token("Product.ShortDescription", await _localizationService.GetLocalizedAsync(product, (Product x) => x.ShortDescription, languageId), neverHtmlEncoded: true));
		tokens.Add(new Token("Product.SKU", product.Sku));
		list = tokens;
		list.Add(new Token("Product.StockQuantity", await productService.GetTotalStockQuantityAsync(product)));
		await RouteUrlAsync(0, "Product", new
		{
			SeName = await _urlRecordService.GetSeNameAsync(product)
		});
		await _eventPublisher.EntityTokensAddedAsync(product, tokens);
	}

	public async Task AddReturnRequestTokensAsync(IList<Token> tokens, ReturnRequest returnRequest, OrderItem orderItem)
	{
		Product product = await _productService.GetProductByIdAsync(orderItem.ProductId);
		tokens.Add(new Token("ReturnRequest.CustomNumber", returnRequest.CustomNumber));
		tokens.Add(new Token("ReturnRequest.OrderId", orderItem.OrderId));
		tokens.Add(new Token("ReturnRequest.Product.Quantity", returnRequest.Quantity));
		tokens.Add(new Token("ReturnRequest.Product.Name", product.Name));
		tokens.Add(new Token("ReturnRequest.Reason", returnRequest.ReasonForReturn));
		tokens.Add(new Token("ReturnRequest.RequestedAction", returnRequest.RequestedAction));
		tokens.Add(new Token("ReturnRequest.CustomerComment", _htmlFormatter.FormatText(returnRequest.CustomerComments, stripTags: false, convertPlainTextToHtml: true, allowHtml: false, allowBbCode: false, resolveLinks: false, addNoFollowTag: false), neverHtmlEncoded: true));
		tokens.Add(new Token("ReturnRequest.StaffNotes", _htmlFormatter.FormatText(returnRequest.StaffNotes, stripTags: false, convertPlainTextToHtml: true, allowHtml: false, allowBbCode: false, resolveLinks: false, addNoFollowTag: false), neverHtmlEncoded: true));
		tokens.Add(new Token("ReturnRequest.Status", await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus)));
		await _eventPublisher.EntityTokensAddedAsync(returnRequest, tokens);
	}

	public async Task AddForumTopicTokensAsync(IList<Token> tokens, ForumTopic forumTopic, int? friendlyForumTopicPageIndex = null, int? appendedPostIdentifierAnchor = null)
	{
		IForumService forumService = NopInstance.Load<IForumService>();
		string value;
		if (friendlyForumTopicPageIndex.HasValue && friendlyForumTopicPageIndex.Value > 1)
		{
			int id = forumTopic.Id;
			value = await RouteUrlAsync(0, "TopicSlugPaged", new
			{
				id = id,
				slug = await forumService.GetTopicSeNameAsync(forumTopic),
				pageNumber = friendlyForumTopicPageIndex.Value
			});
		}
		else
		{
			int id = forumTopic.Id;
			value = await RouteUrlAsync(0, "TopicSlug", new
			{
				id = id,
				slug = await forumService.GetTopicSeNameAsync(forumTopic)
			});
		}
		if (appendedPostIdentifierAnchor.HasValue && appendedPostIdentifierAnchor.Value > 0)
		{
			value = $"{value}#{appendedPostIdentifierAnchor.Value}";
		}
		tokens.Add(new Token("Forums.TopicURL", value, neverHtmlEncoded: true));
		tokens.Add(new Token("Forums.TopicName", forumTopic.Subject));
		await _eventPublisher.EntityTokensAddedAsync(forumTopic, tokens);
	}

	public async Task AddForumTokensAsync(IList<Token> tokens, Forum forum)
	{
		IForumService forumService = NopInstance.Load<IForumService>();
		int id = forum.Id;
		tokens.Add(new Token("Forums.ForumURL", await RouteUrlAsync(0, "ForumSlug", new
		{
			id = id,
			slug = await forumService.GetForumSeNameAsync(forum)
		}), neverHtmlEncoded: true));
		tokens.Add(new Token("Forums.ForumName", forum.Name));
		await _eventPublisher.EntityTokensAddedAsync(forum, tokens);
	}

	public async Task AddAttributeCombinationTokensAsync(IList<Token> tokens, ProductAttributeCombination combination, int languageId)
	{
		IProductAttributeFormatter productAttributeFormatter = NopInstance.Load<IProductAttributeFormatter>();
		IProductService productService = NopInstance.Load<IProductService>();
		Product product = await _productService.GetProductByIdAsync(combination.ProductId);
		tokens.Add(new Token("AttributeCombination.Formatted", await productAttributeFormatter.FormatAttributesAsync(product, combination.AttributesXml), neverHtmlEncoded: true));
		tokens.Add(new Token("AttributeCombination.SKU", await productService.FormatSkuAsync(product, combination.AttributesXml)));
		tokens.Add(new Token("AttributeCombination.StockQuantity", combination.StockQuantity));
		await _eventPublisher.EntityTokensAddedAsync(combination, tokens);
	}

	public async Task AddForumPostTokensAsync(IList<Token> tokens, ForumPost forumPost)
	{
		Customer customer = await _customerService.GetCustomerByIdAsync(forumPost.CustomerId);
		IForumService forumService = NopInstance.Load<IForumService>();
		tokens.Add(new Token("Forums.PostAuthor", await _customerService.FormatUsernameAsync(customer)));
		tokens.Add(new Token("Forums.PostBody", forumService.FormatPostText(forumPost), neverHtmlEncoded: true));
		await _eventPublisher.EntityTokensAddedAsync(forumPost, tokens);
	}

	public async Task AddPrivateMessageTokensAsync(IList<Token> tokens, PrivateMessage privateMessage)
	{
		IForumService forumService = NopInstance.Load<IForumService>();
		tokens.Add(new Token("PrivateMessage.Subject", privateMessage.Subject));
		tokens.Add(new Token("PrivateMessage.Text", forumService.FormatPrivateMessageText(privateMessage), neverHtmlEncoded: true));
		await _eventPublisher.EntityTokensAddedAsync(privateMessage, tokens);
	}

	public async Task AddVendorTokensAsync(IList<Token> tokens, Vendor vendor)
	{
		tokens.Add(new Token("Vendor.Name", vendor.Name));
		tokens.Add(new Token("Vendor.Email", vendor.Email));
		string attributesXml = await _genericAttributeService.GetAttributeAsync<string>(vendor, NopVendorDefaults.VendorAttributes);
		tokens.Add(new Token("Vendor.VendorAttributes", await _vendorAttributeFormatter.FormatAttributesAsync(attributesXml), neverHtmlEncoded: true));
		await _eventPublisher.EntityTokensAddedAsync(vendor, tokens);
	}

	public async Task AddProductReviewTokensAsync(IList<Token> tokens, ProductReview productReview)
	{
		tokens.Add(new Token("ProductReview.ProductName", (await _productService.GetProductByIdAsync(productReview.ProductId))?.Name));
		tokens.Add(new Token("ProductReview.Title", productReview.Title));
		tokens.Add(new Token("ProductReview.IsApproved", productReview.IsApproved));
		tokens.Add(new Token("ProductReview.ReviewText", productReview.ReviewText));
		tokens.Add(new Token("ProductReview.ReplyText", productReview.ReplyText));
		await _eventPublisher.EntityTokensAddedAsync(productReview, tokens);
	}
}
