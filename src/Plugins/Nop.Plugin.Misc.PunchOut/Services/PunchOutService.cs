using System.Security.Cryptography;
using System.Text;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.PunchOut.Domain;
using Nop.Plugin.Misc.PunchOut.Domain.CXML;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.PunchOut.Services;

/// <summary>
/// Represents the service to manage PunchOut operations
/// </summary>
public class PunchOutService
{
    #region Fields

    protected readonly IAddressService _addressService;
    protected readonly ICurrencyService _currencyService;
    protected readonly ICountryService _countryService;
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected readonly ICustomerService _customerService;
    protected readonly ICustomNumberFormatter _customNumberFormatter;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ILogger _logger;
    protected readonly IOrderService _orderService;
    protected readonly IPriceCalculationService _priceCalculationService;
    protected readonly IProductService _productService;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IStoreContext _storeContext;
    protected readonly IWebHelper _webHelper;
    protected readonly IWorkContext _workContext;
    protected readonly PunchOutIdentityService _punchOutIdentityService;
    protected readonly PunchOutLogService _punchOutLogService;
    protected readonly PunchOutXmlBuilder _punchOutXmlBuilder;

    #endregion

    #region Ctor

    public PunchOutService(IAddressService addressService,
        ICurrencyService currencyService,
        ICountryService countryService,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        ICustomNumberFormatter customNumberFormatter,
        IGenericAttributeService genericAttributeService,
        ILogger logger,
        IOrderService orderService,
        IPriceCalculationService priceCalculationService,
        IProductService productService,
        IShoppingCartService shoppingCartService,
        IStateProvinceService stateProvinceService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IWebHelper webHelper,
        IWorkContext workContext,
        PunchOutIdentityService punchOutIdentityService,
        PunchOutLogService punchOutLogService,
        PunchOutXmlBuilder punchOutXmlBuilder)
    {
        _addressService = addressService;
        _currencyService = currencyService;
        _countryService = countryService;
        _customerRegistrationService = customerRegistrationService;
        _customerService = customerService;
        _customNumberFormatter = customNumberFormatter;
        _genericAttributeService = genericAttributeService;
        _logger = logger;
        _orderService = orderService;
        _priceCalculationService = priceCalculationService;
        _productService = productService;
        _shoppingCartService = shoppingCartService;
        _stateProvinceService = stateProvinceService;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _workContext = workContext;
        _punchOutIdentityService = punchOutIdentityService;
        _punchOutLogService = punchOutLogService;
        _punchOutXmlBuilder = punchOutXmlBuilder;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Validates the sender of the PunchOut message
    /// </summary>
    /// <param name="identity">The identity of the sender</param>
    /// <param name="sharedSecret">The incoming shared secret</param>
    /// <param name="payloadId">The payload ID</param>
    /// <returns>The XML string</returns>
    private async Task<string> ValidateSenderAsync(string identity, string sharedSecret, string payloadId)
    {
        var sender = await _punchOutIdentityService.GetPunchOutIdentitiesAsync(identity)
                ?? throw new NopException("Unknown PunchOut identity.");

        // client validation verification
        var storedIdentity = sender?.FirstOrDefault().SharedSecretHash;
        var incomingSecret = sharedSecret;

        var incomingBytes = Encoding.UTF8.GetBytes(incomingSecret);
        var expectedBytes = Encoding.UTF8.GetBytes(storedIdentity);

        if (!CryptographicOperations.FixedTimeEquals(incomingBytes, expectedBytes))
        {
            var errorXml = _punchOutXmlBuilder.BuildErrorResponse(new PunchOutErrorResponse
            {
                StatusCode = "401",
                StatusText = "Authentication Failed",
                ErrorMessage = "Invalid shared secret"
            });

            await _punchOutLogService.LogAsync(new PunchOutLog
            {
                PayloadId = payloadId,
                MessageTypeId = (int)PunchOutMessageType.SetupRequest,
                DirectionId = (int)PunchOutDirection.Outbound,
                RawXml = errorXml,
                Error = "Invalid shared secret"
            });

            return errorXml;
        }

        return string.Empty;
    }

    /// <summary>
    /// Generates a secure random token for PunchOut session identification
    /// </summary>
    /// <param name="length">The length of the token to generate</param>
    /// <returns>The generated token</returns>
    private static string GenerateSecureToken(int length)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_")[..length];
    }

    /// <summary>
    /// Creates a new customer for PunchOut session based on the provided email
    /// </summary>
    /// <param name="customerEmail">The email of the customer to create</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer
    /// </returns>
    private async Task<Customer> CreatePunchOutCustomerAsync(string customerEmail)
    {
        var customer = new Customer
        {
            Email = customerEmail,
            Username = customerEmail,
            Active = true,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _customerService.InsertCustomerAsync(customer);
        await _customerService.InsertCustomerPasswordAsync(new CustomerPassword
        {
            CustomerId = customer.Id,
            PasswordFormat = PasswordFormat.Clear,
            Password = Guid.NewGuid().ToString("N")
        });

        var role = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        if (role != null)
        {
            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = role.Id });
        }

        return customer;
    }

    /// <summary>
    /// Creates or updates a customer address based on PunchOut address data
    /// </summary>
    /// <param name="punchOutAddress">The PunchOut address data</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the created or updated address
    /// </returns>
    private async Task<Address> CreateOrUpdateAddressAsync(PunchOutAddress punchOutAddress)
    {
        ArgumentNullException.ThrowIfNull(punchOutAddress);

        // Get country by ISO code
        int? countryId = null;
        int? stateProvinceId = null;

        if (!string.IsNullOrEmpty(punchOutAddress.Country))
        {
            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(punchOutAddress.Country);
            countryId = country?.Id;
        }

        if (countryId.HasValue && !string.IsNullOrEmpty(punchOutAddress.State))
        {
            var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(punchOutAddress.State, countryId.Value);
            stateProvinceId = state?.Id;
        }

        var address = new Address
        {
            FirstName = ExtractFirstName(punchOutAddress.Name),
            LastName = ExtractLastName(punchOutAddress.Name),
            Email = punchOutAddress.Email,
            Company = punchOutAddress.Company,
            CountryId = countryId,
            StateProvinceId = stateProvinceId,
            City = punchOutAddress.City,
            Address1 = punchOutAddress.Address1,
            Address2 = punchOutAddress.Address2,
            ZipPostalCode = punchOutAddress.PostalCode,
            PhoneNumber = punchOutAddress.PhoneNumber,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _addressService.InsertAddressAsync(address);
        return address;
    }

    /// <summary>
    /// Extracts first name from full name
    /// </summary>
    /// <param name="fullName">The full name</param>
    /// <returns>The first name</returns>
    private string ExtractFirstName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return string.Empty;

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    /// <summary>
    /// Extracts last name from full name
    /// </summary>
    /// <param name="fullName">The full name</param>
    /// <returns>The last name</returns>
    private string ExtractLastName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return string.Empty;

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handles the incoming PunchOutSetupRequest
    /// </summary>
    /// <param name="xml">The XML string representing the setup request</param>
    /// <param name="httpContext">The HTTP context</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the response XML
    /// </returns>
    public async Task<string> HandleSetupRequestAsync(string xml, HttpContext httpContext)
    {
        var request = _punchOutXmlBuilder.ParseSetupRequest(xml);
        var sessionId = GenerateSecureToken(PunchOutDefaults.TokenLength);

        try
        {
            // inbound log
            await _punchOutLogService.LogAsync(new PunchOutLog
            {
                SessionId = sessionId,
                BuyerCookie = request.BuyerCookie,
                Identity = request.Identity,
                PayloadId = request.PayloadId,
                MessageTypeId = (int)PunchOutMessageType.SetupRequest,
                DirectionId = (int)PunchOutDirection.Inbound,
                RawXml = xml,
                Url = httpContext.Request.Path,
                HttpMethod = httpContext.Request.Method
            });

            var validationError = await ValidateSenderAsync(request.Identity, request.SharedSecret, request.PayloadId);
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }

            // customer creation
            var contactEmail = string.IsNullOrEmpty(request.Contact)
                ? throw new NopException("Customer email not found.")
                : request.Contact;

            var customer = await _customerService.GetCustomerByEmailAsync(contactEmail);
            if (customer is null && !string.IsNullOrWhiteSpace(contactEmail))
            {
                customer = await CreatePunchOutCustomerAsync(contactEmail);
            }

            // PunchOut session
            var session = new PunchOutSession
            {
                SessionId = sessionId,
                BuyerCookie = request.BuyerCookie,
                ReturnUrl = request.BrowserFormPostUrl,
                CustomerId = customer.Id,
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                IsActive = false,
                CreatedOnUtc = DateTime.UtcNow
            };

            await SavePunchoutSessionAsync(customer, session);

            //TODO: only for testing
            var storeLocation = "https://penni-cormlike-overscrupulously.ngrok-free.dev/";
            //_webHelper.GetStoreLocation();
            var startUrl = $"{storeLocation}punchout/start?sessionId={sessionId}";
            var responseXml = _punchOutXmlBuilder.BuildSetupResponse(
                new PunchOutSetupResponse
                {
                    SessionId = sessionId,
                    StartPageUrl = startUrl
                });

            // outbound log
            await _punchOutLogService.LogAsync(new PunchOutLog
            {
                PayloadId = request.PayloadId,
                SessionId = sessionId,
                MessageTypeId = (int)PunchOutMessageType.SetupResponse,
                DirectionId = (int)PunchOutDirection.Outbound,
                RawXml = responseXml,
            });

            return responseXml;
        }
        catch (Exception ex)
        {
            await _punchOutLogService.LogAsync(new PunchOutLog
            {
                SessionId = sessionId,
                PayloadId = request.PayloadId,
                MessageTypeId = (int)PunchOutMessageType.SetupRequest,
                DirectionId = (int)PunchOutDirection.Inbound,
                RawXml = xml,
                Error = ex.ToString()
            });

            return _punchOutXmlBuilder.BuildErrorResponse(new PunchOutErrorResponse
            {
                StatusCode = "400",
                StatusText = "Bad Request",
                ErrorMessage = ex.Message
            });
        }
    }

    /// <summary>
    /// Handles the incoming PunchOutOrderRequest
    /// </summary>
    /// <param name="xml">The XML string representing the order request</param>
    /// <param name="httpContext">The HTTP context</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the response XML
    /// </returns>
    public async Task<string> HandleOrderRequestAsync(string xml, HttpContext httpContext)
    {
        var request = _punchOutXmlBuilder.ParseOrderRequest(xml);

        try
        {
            // inbound log
            await _punchOutLogService.LogAsync(new PunchOutLog
            {
                PayloadId = request.PayloadId,
                MessageTypeId = (int)PunchOutMessageType.OrderRequest,
                DirectionId = (int)PunchOutDirection.Inbound,
                RawXml = xml,
                Url = httpContext.Request.Path,
                HttpMethod = httpContext.Request.Method
            });

            var validationError = await ValidateSenderAsync(request.Identity, request.SharedSecret, request.PayloadId);
            if (!string.IsNullOrEmpty(validationError))
            {
                return validationError;
            }

            // customer creation
            var contactEmail = request.Contact;
            var customer = await _customerService.GetCustomerByEmailAsync(contactEmail);
            if (customer is null && !string.IsNullOrWhiteSpace(contactEmail))
            {
                customer = await CreatePunchOutCustomerAsync(contactEmail);
            }

            var store = await _storeContext.GetCurrentStoreAsync();

            // Add or update addresses for customer
            if (request.BillTo != null)
            {
                var billToAddress = await CreateOrUpdateAddressAsync(request.BillTo);
                if (customer.BillingAddressId != billToAddress.Id)
                {
                    customer.BillingAddressId = billToAddress.Id;
                    await _customerService.UpdateCustomerAsync(customer);
                }
            }

            if (request.ShipTo != null)
            {
                var shipToAddress = await CreateOrUpdateAddressAsync(request.ShipTo);
                if (customer.ShippingAddressId != shipToAddress.Id)
                {
                    customer.ShippingAddressId = shipToAddress.Id;
                    await _customerService.UpdateCustomerAsync(customer);
                }
            }

            // Calculate order total from line items
            var orderTotal = request.LineItems.Sum(item => item.UnitPrice * item.Quantity);

            // Create order
            var order = new Order
            {
                StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                OrderGuid = Guid.TryParse(request.OrderID, out var orderGuid) ? orderGuid : Guid.NewGuid(),
                CustomerId = customer.Id,
                CustomerLanguageId = customer.LanguageId ?? 0,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                OrderSubtotalInclTax = orderTotal,
                OrderTotal = orderTotal,
                RefundedAmount = decimal.Zero,
                OrderDiscount = decimal.Zero,
                CustomerCurrencyCode = request.CurrencyCode,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                PaidDateUtc = null,
                ShippingStatus = ShippingStatus.NotYetShipped,
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = string.Empty,
                BillingAddressId = customer.BillingAddressId ?? 0,
                ShippingAddressId = customer.ShippingAddressId ?? 0,
                TaxRates = string.Empty,
                OrderTax = 0m,
                CurrencyRate = 1m,
                AuthorizationTransactionId = request.PayloadId,
                CaptureTransactionId = request.PayloadId,

            };

            await _orderService.InsertOrderAsync(order);

            //generate and set custom order number
            order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
            await _orderService.UpdateOrderAsync(order);

            // Add order items from XML request
            foreach (var lineItem in request.LineItems)
            {
                // Find product by SKU
                var product = await _productService.GetProductBySkuAsync(lineItem.SupplierPartId)
                    ?? throw new NopException($"Product with SKU '{lineItem.SupplierPartId}' not found.");

                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = lineItem.Quantity,
                    UnitPriceInclTax = lineItem.UnitPrice,
                    UnitPriceExclTax = lineItem.UnitPrice,
                    PriceInclTax = lineItem.UnitPrice * lineItem.Quantity,
                    PriceExclTax = lineItem.UnitPrice * lineItem.Quantity,
                    OriginalProductCost = product.ProductCost,
                    ItemWeight = product.Weight,
                    AttributeDescription = lineItem.Description,
                    DiscountAmountInclTax = decimal.Zero,
                    DiscountAmountExclTax = decimal.Zero,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0
                };

                await _orderService.InsertOrderItemAsync(orderItem);
            }

            var responseXml = _punchOutXmlBuilder.BuildOrderResponse(
                new PunchOutOrderResponse
                {
                    StatusCode = "200",
                    StatusText = "OK"
                });

            // outbound log
            await _punchOutLogService.LogAsync(new PunchOutLog
            {
                PayloadId = request.PayloadId,
                MessageTypeId = (int)PunchOutMessageType.OrderResponse,
                DirectionId = (int)PunchOutDirection.Outbound,
                RawXml = responseXml,
            });

            return responseXml;
        }
        catch (Exception ex)
        {
            await _punchOutLogService.LogAsync(new PunchOutLog
            {
                PayloadId = request.PayloadId,
                MessageTypeId = (int)PunchOutMessageType.OrderRequest,
                DirectionId = (int)PunchOutDirection.Inbound,
                RawXml = xml,
                Error = ex.ToString()
            });

            return _punchOutXmlBuilder.BuildErrorResponse(new PunchOutErrorResponse
            {
                StatusCode = "500",
                StatusText = "Internal Error",
                ErrorMessage = ex.Message
            });
        }
    }

    /// <summary>
    /// Activates the PunchOut session and returns session details for the storefront to start the session
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the PunchOut session details and associated customer information
    /// </returns>
    public async Task<PunchOutSessionStartResult> StartSessionAsync(string sessionId)
    {
        var session = await GetPunchOutSessionByIdAsync(sessionId)
            ?? throw new NopException("PunchOut session not found.");

        if (session != null && !string.IsNullOrEmpty(session.SessionId) && session.CustomerId != 0)
        {
            var customer = await _customerService.GetCustomerByIdAsync(session.CustomerId);

            session.IsActive = true;
            session.CreatedOnUtc = DateTime.UtcNow;

            await SavePunchoutSessionAsync(customer, session);

            return new PunchOutSessionStartResult
            {
                Session = session,
                Customer = customer
            };
        }
        return new PunchOutSessionStartResult();
    }

    /// <summary>
    /// Builds the PunchOut response with the PunchOutOrderMessage XML payload
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the PunchOutReturnResponse
    /// </returns>
    public async Task<PunchOutReturnResponse> BuildReturnResponseAsync()
    {
        var session = await GetPunchOutSessionAsync();
        var cxml = await BuildOrderMessageAsync();
        var html = _punchOutXmlBuilder.BuildAutoSubmitForm(session.ReturnUrl, cxml);

        return new PunchOutReturnResponse
        {
            SessionId = session.SessionId,
            Html = html
        };
    }

    /// <summary>
    /// Builds the PunchOutOrderMessage XML based on the current customer's shopping cart
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the response XML
    /// </returns>
    public async Task<string> BuildOrderMessageAsync()
    {
        var session = await GetPunchOutSessionAsync()
            ?? throw new NopException("PunchOut session not found.");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);
        var model = new PunchOutOrderMessage
        {
            BuyerCookie = session.BuyerCookie
        };

        var total = 0m;

        foreach (var item in cart)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            var currency = await _currencyService.GetCurrencyByIdAsync(customer.CurrencyId ?? 0)
                ?? await _workContext.GetWorkingCurrencyAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var (_, finalPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store);
            total += finalPrice * item.Quantity;

            model.Items.Add(new PunchOutOrderItem
            {
                SupplierPartId = product.Sku,
                Description = product.Name,
                Quantity = item.Quantity,
                UnitPrice = finalPrice,
                CurrencyCode = currency.CurrencyCode
            });
        }

        model.Total = total;

        return _punchOutXmlBuilder.BuildPunchOutOrderMessage(model);
    }

    #region Session

    /// <summary>
    /// Checks if the current customer has an active PunchOut session
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result indicates whether the customer has an active PunchOut session
    /// </returns>
    public async Task<bool> IsPunchoutSessionAsync()
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var isActiveSession = await _genericAttributeService.GetAttributeAsync<bool>(customer,
                PunchOutDefaults.PunchOutIsActiveAttribute, store.Id);

            return isActiveSession;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PunchOut: error determining session state");
            return false;
        }
    }

    /// <summary>
    /// Gets all saved punchout session data for the customer
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the PunchOut session details
    /// </returns>
    public async Task<PunchOutSession> GetPunchOutSessionAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        try
        {
            var sessionToken = await _genericAttributeService.GetAttributeAsync<string>(customer, PunchOutDefaults.PunchOutSessionTokenAttribute, store.Id);
            var returnUrl = await _genericAttributeService.GetAttributeAsync<string>(customer, PunchOutDefaults.PunchOutReturnUrlAttribute, store.Id);
            var buyerCookie = await _genericAttributeService.GetAttributeAsync<string>(customer, PunchOutDefaults.PunchOutBuyerCookieAttribute, store.Id);
            var startDate = await _genericAttributeService.GetAttributeAsync<DateTime?>(customer, PunchOutDefaults.PunchOutStartDateAttribute, store.Id);
            var isActive = await _genericAttributeService.GetAttributeAsync<bool>(customer, PunchOutDefaults.PunchOutIsActiveAttribute, store.Id);

            return new PunchOutSession
            {
                SessionId = sessionToken ?? string.Empty,
                ReturnUrl = returnUrl ?? string.Empty,
                BuyerCookie = buyerCookie ?? string.Empty,
                CreatedOnUtc = startDate ?? DateTime.UtcNow,
                IsActive = isActive && !string.IsNullOrEmpty(sessionToken)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PunchOut: error retrieving saved punchout session for customer {CustomerId}", customer.Id);
            return new PunchOutSession { IsActive = true };
        }
    }

    public async Task<PunchOutSession> GetPunchOutSessionByIdAsync(string sessionId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(PunchOutDefaults.SessionTokenCacheKey, sessionId);

        return await _staticCacheManager.GetAsync(key, () => Task.FromResult<PunchOutSession>(null));
    }

    /// <summary>
    /// Save punchout session data to customer attributes
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="session">PunchOut session</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the session token
    /// </returns>
    public async Task<string> SavePunchoutSessionAsync(Customer customer, PunchOutSession session)
    {
        var token = session.SessionId;
        await _genericAttributeService.SaveAttributeAsync(customer, PunchOutDefaults.PunchOutSessionTokenAttribute,
            token ?? string.Empty, session.StoreId);

        await _genericAttributeService.SaveAttributeAsync(customer, PunchOutDefaults.PunchOutReturnUrlAttribute,
            session.ReturnUrl, session.StoreId);

        await _genericAttributeService.SaveAttributeAsync(customer, PunchOutDefaults.PunchOutBuyerCookieAttribute,
            session.BuyerCookie ?? string.Empty, session.StoreId);

        await _genericAttributeService.SaveAttributeAsync(customer, PunchOutDefaults.PunchOutStartDateAttribute,
            session.CreatedOnUtc, session.StoreId);

        await _genericAttributeService.SaveAttributeAsync(customer, PunchOutDefaults.PunchOutIsActiveAttribute,
            session.IsActive, session.StoreId);

        //save session data to cache for quick retrieval during the session
        var key = _staticCacheManager.PrepareKeyForDefaultCache(PunchOutDefaults.SessionTokenCacheKey, session.SessionId);
        await _staticCacheManager.SetAsync(key, session);

        return token;
    }

    /// <summary>
    /// Clear all punchout session data for the customer
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// </returns>
    public async Task ClearPunchoutSessionDataAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        try
        {
            var sessionToken = await _genericAttributeService.GetAttributeAsync<string>(customer,
                PunchOutDefaults.PunchOutSessionTokenAttribute, store.Id);

            await _genericAttributeService.SaveAttributeAsync<string>(customer, PunchOutDefaults.PunchOutSessionTokenAttribute, null, store.Id);
            await _genericAttributeService.SaveAttributeAsync<string>(customer, PunchOutDefaults.PunchOutReturnUrlAttribute, null, store.Id);
            await _genericAttributeService.SaveAttributeAsync<string>(customer, PunchOutDefaults.PunchOutBuyerCookieAttribute, null, store.Id);
            await _genericAttributeService.SaveAttributeAsync<string>(customer, PunchOutDefaults.PunchOutStartDateAttribute, null, store.Id);
            await _genericAttributeService.SaveAttributeAsync(customer, PunchOutDefaults.PunchOutIsActiveAttribute, false, store.Id);

            //clear cache
            var key = _staticCacheManager.PrepareKeyForDefaultCache(PunchOutDefaults.SessionTokenCacheKey, sessionToken);
            await _staticCacheManager.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PunchOut: error clearing punchout session data for customer {CustomerId}", customer.Id);
        }
    }

    #endregion

    #endregion
}
