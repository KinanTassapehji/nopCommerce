using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.FilterLevels;
using Nop.Core.Events;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Events;

namespace Nop.Web.Framework.Menu;

/// <summary>
/// Admin menu
/// </summary>
public partial class AdminMenu : IAdminMenu
{
    #region Fields

    protected AdminMenuItem _baseRootMenuItem;
    protected AdminMenuItem _rootItem;

    protected readonly FilterLevelSettings _filterLevelSettings;
    protected readonly IActionContextAccessor _actionContextAccessor;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly ILocalizationService _localizationService;
    protected readonly IPermissionService _permissionService;
#pragma warning disable CS0618 // Type or member is obsolete
    protected readonly IPluginManager<IAdminMenuPlugin> _adminMenuPluginManager;
#pragma warning restore CS0618 // Type or member is obsolete
    protected readonly IUrlHelperFactory _urlHelperFactory;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    /// <summary>
    /// Ctor
    /// </summary>
    public AdminMenu(FilterLevelSettings filterLevelSettings,
        IActionContextAccessor actionContextAccessor,
        IEventPublisher eventPublisher,
        ILocalizationService localizationService,
        IPermissionService permissionService,
#pragma warning disable CS0618 // Type or member is obsolete
        IPluginManager<IAdminMenuPlugin> adminMenuPluginManager,
#pragma warning restore CS0618 // Type or member is obsolete
        IUrlHelperFactory urlHelperFactory,
        IWorkContext workContext)
    {
        _filterLevelSettings = filterLevelSettings;
        _actionContextAccessor = actionContextAccessor;
        _eventPublisher = eventPublisher;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _adminMenuPluginManager = adminMenuPluginManager;
        _urlHelperFactory = urlHelperFactory;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Fills the base root menu item data
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task FillBaseRootAsync()
    {
        if (_baseRootMenuItem != null)
            return;

        _baseRootMenuItem = new AdminMenuItem
        {
            SystemName = "Home",
            Title = await _localizationService.GetResourceAsync("Admin.Home"),
            Url = GetMenuItemUrl("Home", "Overview"),
            ChildNodes = new List<AdminMenuItem>
            {
                //dashboard
                new()
                {
                    SystemName = "Dashboard",
                    Title = await _localizationService.GetResourceAsync("Admin.Dashboard"),
                    //without its own permission it inherits the root's, which is every permission in the menu,
                    //so it would vanish for anyone missing a single super-administrator-only one
                    PermissionNames = new List<string> { StandardPermission.Security.ACCESS_ADMIN_PANEL },
                    Url = GetMenuItemUrl("Home", "Index"),
                    IconClass = "fas fa-desktop"
                },
                //sales
                new()
                {
                    SystemName = "Sales",
                    Title = await _localizationService.GetResourceAsync("Admin.Sales"),
                    IconClass = "fas fa-shopping-cart",
                    ChildNodes = new List<AdminMenuItem>
                    {
                        new()
                        {
                            SystemName = "Orders",
                            Title = await _localizationService.GetResourceAsync("Admin.Orders"),
                            PermissionNames = new List<string> { StandardPermission.Orders.ORDERS_VIEW },
                            Url = GetMenuItemUrl("Order", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Shipments",
                            Title = await _localizationService.GetResourceAsync("Admin.Orders.Shipments.List"),
                            PermissionNames = new List<string> { StandardPermission.Orders.SHIPMENTS_VIEW },
                            Url = GetMenuItemUrl("Order", "ShipmentList"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Return requests",
                            Title = await _localizationService.GetResourceAsync("Admin.ReturnRequests"),
                            PermissionNames = new List<string> { StandardPermission.Orders.RETURN_REQUESTS_VIEW },
                            Url = GetMenuItemUrl("ReturnRequest", "List"),
                            IconClass = "far fa-dot-circle"
                        },

                        new()
                        {
                            SystemName = "Current shopping carts",
                            Title = await _localizationService.GetResourceAsync("Admin.CurrentCarts.CartsAndWishlists"),
                            PermissionNames = new List<string> { StandardPermission.Orders.CURRENT_CARTS_MANAGE },
                            Url = GetMenuItemUrl("ShoppingCart", "CurrentCarts"),
                            IconClass = "far fa-dot-circle"
                        }
                    }
                },
                //catalog
                new()
                {
                    SystemName = "Catalog",
                    Title = await _localizationService.GetResourceAsync("Admin.Catalog"),
                    IconClass = "fas fa-book",
                    ChildNodes = new List<AdminMenuItem>
                    {
                        new()
                        {
                            SystemName = "Products",
                            Title = await _localizationService.GetResourceAsync("Admin.Catalog.Products"),
                            PermissionNames = new List<string> { StandardPermission.Catalog.PRODUCTS_VIEW },
                            Url = GetMenuItemUrl("Product", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Categories",
                            Title = await _localizationService.GetResourceAsync("Admin.Catalog.Categories"),
                            PermissionNames = new List<string> { StandardPermission.Catalog.CATEGORIES_VIEW },
                            Url = GetMenuItemUrl("Category", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Manufacturers",
                            Title = await _localizationService.GetResourceAsync("Admin.Catalog.Manufacturers"),
                            PermissionNames = new List<string> { StandardPermission.Catalog.MANUFACTURER_VIEW },
                            Url = GetMenuItemUrl("Manufacturer", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Product attributes",
                            Title = await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes"),
                            Url = GetMenuItemUrl("ProductAttribute", "List"),
                            PermissionNames = new List<string> { StandardPermission.Catalog.PRODUCT_ATTRIBUTES_VIEW },
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Product tags",
                            Title = await _localizationService.GetResourceAsync("Admin.Catalog.ProductTags"),
                            PermissionNames = new List<string> { StandardPermission.Catalog.PRODUCT_TAGS_VIEW },
                            Url = GetMenuItemUrl("Product", "ProductTags"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Filter level values",
                            Title = await _localizationService.GetResourceAsync("Admin.Catalog.FilterLevelValues"),
                            PermissionNames = new List<string> { StandardPermission.Catalog.FILTER_LEVEL_VALUE_VIEW },
                            Url = GetMenuItemUrl("FilterLevelValue", "List"),
                            Visible = _filterLevelSettings.FilterLevelEnabled,
                            IconClass = "far fa-dot-circle"
                        }
                    }
                },
                //customers
                new()
                {
                    SystemName = "Customers",
                    Title = await _localizationService.GetResourceAsync("Admin.Customers"),
                    IconClass = "far fa-user",
                    ChildNodes = new List<AdminMenuItem>
                    {
                        new()
                        {
                            SystemName = "Customers list",
                            Title = await _localizationService.GetResourceAsync("Admin.Customers.Customers"),
                            PermissionNames = new List<string> { StandardPermission.Customers.CUSTOMERS_VIEW },
                            Url = GetMenuItemUrl("Customer", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Online customers",
                            Title = await _localizationService.GetResourceAsync("Admin.Customers.OnlineCustomers"),
                            PermissionNames = new List<string> { StandardPermission.Customers.CUSTOMERS_VIEW },
                            Url = GetMenuItemUrl("OnlineCustomer", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Customer roles",
                            Title = await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles"),
                            PermissionNames = new List<string> { StandardPermission.Customers.CUSTOMER_ROLES_VIEW },
                            Url = GetMenuItemUrl("CustomerRole", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Activity logs",
                            Title = await _localizationService.GetResourceAsync("Admin.Customers.ActivityLog"),
                            PermissionNames = new List<string> { StandardPermission.Customers.ACTIVITY_LOG_VIEW },
                            Url = GetMenuItemUrl("ActivityLog", "ActivityLogs"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Activity types",
                            Title = await _localizationService.GetResourceAsync("Admin.Customers.ActivityLogType"),
                            PermissionNames = new List<string> { StandardPermission.Customers.ACTIVITY_LOG_VIEW },
                            Url = GetMenuItemUrl("ActivityLog", "ActivityTypes"),
                            IconClass = "far fa-dot-circle"
                        }
                    }
                },
                //marketing
                new()
                {
                    SystemName = "Marketing",
                    Title = await _localizationService.GetResourceAsync("Admin.Marketing"),
                    IconClass = "fas fa-bullhorn",
                    ChildNodes = new List<AdminMenuItem>
                    {
                        //affiliates, newsletters and campaigns are not used; the push-notification
                        //broadcast plugin adds itself here
                        new()
                        {
                            SystemName = "Discounts",
                            Title = await _localizationService.GetResourceAsync("Admin.Promotions.Discounts"),
                            PermissionNames = new List<string> { StandardPermission.Promotions.DISCOUNTS_VIEW },
                            Url = GetMenuItemUrl("Discount", "List"),
                            IconClass = "far fa-dot-circle"
                        }
                    }
                },
                //content management
                new()
                {
                    SystemName = "Content Management",
                    Title = await _localizationService.GetResourceAsync("Admin.ContentManagement"),
                    IconClass = "fas fa-cubes",
                    ChildNodes = new List<AdminMenuItem>
                    {
                        new()
                        {
                            SystemName = "Topics",
                            Title = await _localizationService.GetResourceAsync("Admin.ContentManagement.Topics"),
                            PermissionNames = new List<string> { StandardPermission.ContentManagement.TOPICS_VIEW },
                            Url = GetMenuItemUrl("Topic", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Menus",
                            Title = await _localizationService.GetResourceAsync("Admin.ContentManagement.Menus"),
                            PermissionNames = new List<string> { StandardPermission.ContentManagement.MENU_VIEW },
                            Url = GetMenuItemUrl("Menu", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Message templates",
                            Title = await _localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.ContentManagement.MESSAGE_TEMPLATES_VIEW
                                },
                            Url = GetMenuItemUrl("MessageTemplate", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "News items",
                            //news, blog and forums are not used yet; hidden, not removed - flip to true to bring them back
                            Visible = false,
                            Title = await _localizationService.GetResourceAsync("Admin.ContentManagement.News.NewsItems"),
                            PermissionNames =
                                new List<string> { StandardPermission.ContentManagement.NEWS_VIEW },
                            Url = GetMenuItemUrl("News", "NewsItems"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "News comments",
                            //news, blog and forums are not used yet; hidden, not removed - flip to true to bring them back
                            Visible = false,
                            Title = await _localizationService.GetResourceAsync("Admin.ContentManagement.News.Comments"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.ContentManagement.NEWS_COMMENTS_VIEW
                                },
                            Url = GetMenuItemUrl("News", "NewsComments"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Blog posts",
                            //news, blog and forums are not used yet; hidden, not removed - flip to true to bring them back
                            Visible = false,
                            Title = await _localizationService.GetResourceAsync("Admin.ContentManagement.Blog.BlogPosts"),
                            PermissionNames = new List<string> { StandardPermission.ContentManagement.BLOG_VIEW },
                            Url = GetMenuItemUrl("Blog", "BlogPosts"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Blog comments",
                            //news, blog and forums are not used yet; hidden, not removed - flip to true to bring them back
                            Visible = false,
                            Title = await _localizationService.GetResourceAsync("Admin.ContentManagement.Blog.Comments"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.ContentManagement.BLOG_COMMENTS_VIEW
                                },
                            Url = GetMenuItemUrl("Blog", "BlogComments"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Manage forums",
                            //news, blog and forums are not used yet; hidden, not removed - flip to true to bring them back
                            Visible = false,
                            Title = await _localizationService.GetResourceAsync("Admin.ContentManagement.Forums"),
                            PermissionNames = new List<string> { StandardPermission.ContentManagement.FORUMS_VIEW },
                            Url = GetMenuItemUrl("Forum", "List"),
                            IconClass = "far fa-dot-circle"
                        }
                    }
                },
                //reports
                new()
                {
                    SystemName = "Reports",
                    Title = await _localizationService.GetResourceAsync("Admin.Reports"),
                    IconClass = "fas fa-chart-line",
                    ChildNodes = new List<AdminMenuItem>
                    {
                        new()
                        {
                            SystemName = "Sales summary",
                            Title = await _localizationService.GetResourceAsync("Admin.Reports.SalesSummary"),
                            PermissionNames = new List<string> { StandardPermission.Reports.SALES_SUMMARY },
                            Url = GetMenuItemUrl("Report", "SalesSummary"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Low stock",
                            Title = await _localizationService.GetResourceAsync("Admin.Reports.LowStock"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.Catalog.PRODUCTS_VIEW,
                                    StandardPermission.Reports.LOW_STOCK
                                },
                            Url = GetMenuItemUrl("Report", "LowStock"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Bestsellers",
                            Title = await _localizationService.GetResourceAsync("Admin.Reports.Sales.Bestsellers"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.Orders.ORDERS_VIEW,
                                    StandardPermission.Reports.BESTSELLERS
                                },
                            Url = GetMenuItemUrl("Report", "Bestsellers"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Products never purchased",
                            Title = await _localizationService.GetResourceAsync("Admin.Reports.Sales.NeverSold"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.Orders.ORDERS_VIEW,
                                    StandardPermission.Reports.PRODUCTS_NEVER_PURCHASED
                                },
                            Url = GetMenuItemUrl("Report", "NeverSold"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Registered customers",
                            Title = await _localizationService.GetResourceAsync("Admin.Reports.Customers.RegisteredCustomers"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.Customers.CUSTOMERS_VIEW,
                                    StandardPermission.Reports.REGISTERED_CUSTOMERS
                                },
                            Url = GetMenuItemUrl("Report", "RegisteredCustomers"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Customers by order total",
                            Title = await _localizationService.GetResourceAsync("Admin.Reports.Customers.BestBy.BestByOrderTotal"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.Customers.CUSTOMERS_VIEW,
                                    StandardPermission.Reports.CUSTOMERS_BY_ORDER_TOTAL
                                },
                            Url = GetMenuItemUrl("Report", "BestCustomersByOrderTotal"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Customers by number of orders",
                            Title = await _localizationService.GetResourceAsync("Admin.Reports.Customers.BestBy.BestByNumberOfOrders"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.Customers.CUSTOMERS_VIEW,
                                    StandardPermission.Reports.CUSTOMERS_BY_NUMBER_OF_ORDERS
                                },
                            Url = GetMenuItemUrl("Report", "BestCustomersByNumberOfOrders"),
                            IconClass = "far fa-dot-circle"
                        }
                    }
                },
                //configuration
                new()
                {
                    SystemName = "Configuration",
                    Title = await _localizationService.GetResourceAsync("Admin.Configuration"),
                    IconClass = "fas fa-cogs",
                    ChildNodes = new List<AdminMenuItem>
                    {
                        new()
                        {
                            SystemName = "Settings",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings"),
                            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_SETTINGS },
                            IconClass = "far fa-dot-circle",
                            ChildNodes = new List<AdminMenuItem>
                            {
                                new()
                                {
                                    SystemName = "General settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon"),
                                    Url = GetMenuItemUrl("Setting", "GeneralCommon"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Customer and user settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.CustomerUser"),
                                    Url = GetMenuItemUrl("Setting", "CustomerUser"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Order settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Order"),
                                    Url = GetMenuItemUrl("Setting", "Order"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Shipping settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Shipping"),
                                    Url = GetMenuItemUrl("Setting", "Shipping"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Catalog settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Catalog"),
                                    Url = GetMenuItemUrl("Setting", "Catalog"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Filter (YMM) settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.FilterLevel"),
                                    PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_ADVANCED_SETTINGS },
                                    Url = GetMenuItemUrl("Setting", "FilterLevel"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Shopping cart settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.ShoppingCart"),
                                    Url = GetMenuItemUrl("Setting", "ShoppingCart"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "GDPR settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Gdpr"),
                                    PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_ADVANCED_SETTINGS },
                                    Url = GetMenuItemUrl("Setting", "Gdpr"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Blog settings",
                                    //news, blog and forums are not used yet; hidden, not removed - flip to true to bring them back
                                    Visible = false,
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Blog"),
                                    Url = GetMenuItemUrl("Setting", "Blog"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "News settings",
                                    //news, blog and forums are not used yet; hidden, not removed - flip to true to bring them back
                                    Visible = false,
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.News"),
                                    Url = GetMenuItemUrl("Setting", "News"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Forums settings",
                                    //news, blog and forums are not used yet; hidden, not removed - flip to true to bring them back
                                    Visible = false,
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Forums"),
                                    Url = GetMenuItemUrl("Setting", "Forum"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Media settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Media"),
                                    Url = GetMenuItemUrl("Setting", "Media"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "App settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.AppSettings"),
                                    PermissionNames =
                                        new List<string>
                                        {
                                            StandardPermission.System.MANAGE_APP_SETTINGS
                                        },
                                    Url = GetMenuItemUrl("Setting", "AppSettings"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "All settings",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Settings.AllSettings"),
                                    PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_ADVANCED_SETTINGS },
                                    Url = GetMenuItemUrl("Setting", "AllSettings"),
                                    IconClass = "far fa-circle"
                                }
                            }
                        },
                        new()
                        {
                            SystemName = "Currencies",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Currencies"),
                            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_CURRENCIES },
                            Url = GetMenuItemUrl("Currency",
                            "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Payment methods",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Payment.Methods"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.Configuration.MANAGE_PAYMENT_METHODS
                                },
                            Url = GetMenuItemUrl("Payment", "Methods"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Payment restrictions",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Payment.MethodRestrictions"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.Configuration.MANAGE_ADVANCED_SETTINGS
                                },
                            Url = GetMenuItemUrl("Payment", "MethodRestrictions"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Shipping",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Shipping"),
                            PermissionNames =
                                new List<string>
                                {
                                    StandardPermission.Configuration.MANAGE_SHIPPING_SETTINGS
                                },
                            IconClass = "far fa-dot-circle",
                            ChildNodes = new List<AdminMenuItem>
                            {
                                new()
                                {
                                    SystemName = "Shipping providers",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Providers"),
                                    Url = GetMenuItemUrl("Shipping", "Providers"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Warehouses",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Warehouses"),
                                    Url = GetMenuItemUrl("Shipping", "Warehouses"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Pickup points",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.PickupPoints"),
                                    Url = GetMenuItemUrl("Shipping", "PickupPointProviders"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Dates and ranges",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.DatesAndRanges"),
                                    Url = GetMenuItemUrl("Shipping", "DatesAndRanges"),
                                    IconClass = "far fa-circle"
                                },
                                new()
                                {
                                    SystemName = "Measures",
                                    Title = await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Measures"),
                                    Url = GetMenuItemUrl("Measure", "List"),
                                    IconClass = "far fa-circle"
                                }
                            }
                        },
                        new()
                        {
                            SystemName = "Languages",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Languages"),
                            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_LANGUAGES },
                            Url = GetMenuItemUrl("Language",
                            "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Email accounts",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.EmailAccounts"),
                            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_EMAIL_ACCOUNTS },
                            Url = GetMenuItemUrl("EmailAccount",
                            "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Stores",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Stores"),
                            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_STORES },
                            Url = GetMenuItemUrl("Store",
                            "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Countries",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Countries"),
                            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_COUNTRIES },
                            Url = GetMenuItemUrl("Country",
                            "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Access control list",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.ACL"),
                            PermissionNames = new List<string> { StandardPermission.Security.MANAGE_PERMISSIONS },
                            Url = GetMenuItemUrl("Security", "Permissions"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Widgets",
                            Title = await _localizationService.GetResourceAsync("Admin.ContentManagement.Widgets"),
                            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_PLUGIN_AND_WIDGET_LISTS },
                            Url = GetMenuItemUrl("Widget", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Local plugins",
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Plugins.Local"),
                            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_PLUGIN_AND_WIDGET_LISTS },
                            Url = GetMenuItemUrl("Plugin", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "All plugins and themes",
                            //nopCommerce's online marketplace; plugins here are installed from zips; hidden, not removed
                            Visible = false,
                            Title = await _localizationService.GetResourceAsync("Admin.Configuration.Plugins.OfficialFeed"),
                            PermissionNames = new List<string> { StandardPermission.Configuration.MANAGE_PLUGIN_AND_WIDGET_LISTS },
                            Url = GetMenuItemUrl("Plugin", "OfficialFeed"),
                            IconClass = "far fa-dot-circle"
                        }
                    }
                },
                //system
                new()
                {
                    SystemName = "System",
                    Title = await _localizationService.GetResourceAsync("Admin.System"),
                    IconClass = "fas fa-cube",
                    ChildNodes = new List<AdminMenuItem>
                    {
                        new()
                        {
                            SystemName = "System information",
                            //not something the store staff act on; hidden, not removed
                            Visible = false,
                            Title = await _localizationService.GetResourceAsync("Admin.System.SystemInfo"),
                            PermissionNames = new List<string> { StandardPermission.System.MANAGE_MAINTENANCE },
                            Url = GetMenuItemUrl("Common", "SystemInfo"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Log",
                            Title = await _localizationService.GetResourceAsync("Admin.System.Log"),
                            PermissionNames = new List<string> { StandardPermission.System.MANAGE_SYSTEM_LOG },
                            Url = GetMenuItemUrl("Log", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Warnings",
                            Title = await _localizationService.GetResourceAsync("Admin.System.Warnings"),
                            PermissionNames = new List<string> { StandardPermission.System.MANAGE_MAINTENANCE },
                            Url = GetMenuItemUrl("Common", "Warnings"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Maintenance",
                            Title = await _localizationService.GetResourceAsync("Admin.System.Maintenance"),
                            PermissionNames = new List<string> { StandardPermission.System.MANAGE_MAINTENANCE },
                            Url = GetMenuItemUrl("Common", "Maintenance"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Queued emails",
                            Title =
                                await _localizationService.GetResourceAsync("Admin.System.QueuedEmails"),
                            PermissionNames = new List<string> { StandardPermission.System.MANAGE_MESSAGE_QUEUE },
                            Url = GetMenuItemUrl("QueuedEmail", "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Schedule tasks",
                            Title = await _localizationService.GetResourceAsync("Admin.System.ScheduleTasks"),
                            PermissionNames = new List<string> { StandardPermission.System.MANAGE_SCHEDULE_TASKS },
                            Url = GetMenuItemUrl("ScheduleTask",
                            "List"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Search engine friendly names",
                            Title = await _localizationService.GetResourceAsync("Admin.System.SeNames"),
                            PermissionNames = new List<string> { StandardPermission.System.MANAGE_MAINTENANCE },
                            Url = GetMenuItemUrl("Common", "SeNames"),
                            IconClass = "far fa-dot-circle"
                        },
                        new()
                        {
                            SystemName = "Templates",
                            //templates are not managed by the store staff; hidden, not removed
                            Visible = false,
                            Title = await _localizationService.GetResourceAsync("Admin.System.Templates"),
                            PermissionNames = new List<string> { StandardPermission.System.MANAGE_MAINTENANCE },
                            Url = GetMenuItemUrl("Template", "List"),
                            IconClass = "far fa-dot-circle"
                        }
                    }
                },
                //third party plugins
                new()
                {
                    SystemName = "Third party plugins",
                    Title = await _localizationService.GetResourceAsync("Admin.Plugins"),
                    IconClass = "fas fa-bars"
                }
            }
        };
    }

    /// <summary>
    /// Moves a plugin's menu item (found by the end of its URL) after a core menu item.
    /// The item keeps the permissions it had in its old place, so moving it never widens who sees it.
    /// </summary>
    /// <param name="root">Root menu item</param>
    /// <param name="urlSuffix">End of the plugin item's URL</param>
    /// <param name="afterSystemName">System name of the core item to place it after</param>
    /// <param name="takeGroupTitle">Whether the item takes its old group's title (for items named just "List" inside a plugin's group)</param>
    protected virtual void MoveMenuItemAfter(AdminMenuItem root, string urlSuffix, string afterSystemName, bool takeGroupTitle = false)
    {
        List<AdminMenuItem> findPath(AdminMenuItem node)
        {
            if (node.Url?.EndsWith(urlSuffix, StringComparison.OrdinalIgnoreCase) == true)
                return [node];

            foreach (var child in node.ChildNodes)
                if (findPath(child) is { } childPath)
                    return [node, .. childPath];

            return null;
        }

        var path = findPath(root);
        if (path is null || path.Count < 2 || !root.ContainsSystemName(afterSystemName))
            return;

        var item = path[^1];
        if (!item.PermissionNames.Any())
            //plugin items are only on the menu when plugins may be managed (see the "Third party plugins" branch)
            item.PermissionNames = [StandardPermission.Configuration.MANAGE_PLUGINS];

        if (takeGroupTitle)
            item.Title = path[^2].Title;

        path[^2].ChildNodes.Remove(item);
        root.InsertAfter(afterSystemName, item);
    }

    /// <summary>
    /// Gives plugin menu items (found by the end of their URL) a permission of our choosing,
    /// for plugins whose menu code we cannot change
    /// </summary>
    /// <param name="root">Root menu item</param>
    /// <param name="permissionName">Permission the items require</param>
    /// <param name="urlSuffixes">Ends of the items' URLs; none means every item under <paramref name="root"/></param>
    protected virtual void RestrictMenuItems(AdminMenuItem root, string permissionName, params string[] urlSuffixes)
    {
        if (!urlSuffixes.Any() || urlSuffixes.Any(suffix => root.Url?.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) == true))
            root.PermissionNames = [permissionName];

        foreach (var child in root.ChildNodes)
            RestrictMenuItems(child, permissionName, urlSuffixes);
    }

    /// <summary>
    /// Hides plugin menu items (found by the end of their URL) that the store does not use;
    /// the pages themselves stay reachable
    /// </summary>
    /// <param name="root">Root menu item</param>
    /// <param name="urlSuffixes">Ends of the items' URLs</param>
    protected virtual void HideMenuItems(AdminMenuItem root, params string[] urlSuffixes)
    {
        if (urlSuffixes.Any(suffix => root.Url?.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) == true))
            root.Visible = false;

        foreach (var child in root.ChildNodes)
            HideMenuItems(child, urlSuffixes);
    }

    /// <summary>
    /// Replaces every group that holds a single page with that page, titled after the group
    /// (a plugin whose only entry left is "Settings" gets one link instead of a one-item dropdown)
    /// </summary>
    /// <param name="node">Menu item whose descendants are tidied</param>
    protected virtual void CollapseSingleItemGroups(AdminMenuItem node)
    {
        if (node is null)
            return;

        for (var i = 0; i < node.ChildNodes.Count; i++)
        {
            var child = node.ChildNodes[i];
            CollapseSingleItemGroups(child);

            if (child.ChildNodes.Count != 1 || child.ChildNodes[0].ChildNodes.Any())
                continue;

            var onlyItem = child.ChildNodes[0];
            onlyItem.Title = child.Title;
            node.ChildNodes[i] = onlyItem;
        }
    }

    /// <summary>
    /// Loads admin menu
    /// </summary>
    /// <param name="showHidden">A value indicating whether to show hidden records (Visible == false)</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the root menu item for admin menu
    /// </returns>
    protected virtual async Task<AdminMenuItem> LoadMenuAsync(bool showHidden)
    {
        await FillBaseRootAsync();

        AdminMenuItem cloneMenuItem(AdminMenuItem item)
        {
            return new AdminMenuItem
            {
                PermissionNames = item.PermissionNames,
                ChildNodes = item.ChildNodes.Select(cloneMenuItem).ToList(),
                IconClass = item.IconClass,
                Visible = item.Visible,
                OpenUrlInNewTab = item.OpenUrlInNewTab,
                SystemName = item.SystemName,
                Title = item.Title,
                Url = item.Url
            };
        }

        var root = cloneMenuItem(_baseRootMenuItem);

        var customer = await _workContext.GetCurrentCustomerAsync();

        await _eventPublisher.PublishAsync(new AdminMenuCreatedEvent(this, root));

        if (await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS, customer))
        {
            await _eventPublisher.PublishAsync(new ThirdPartyPluginsMenuItemCreatedEvent(this, root.GetItemBySystemName("Third party plugins")));

            var adminMenuPlugins = await _adminMenuPluginManager.LoadAllPluginsAsync(customer);

            foreach (var adminMenuPlugin in adminMenuPlugins)
                await adminMenuPlugin.ManageSiteMapAsync(root);

            //NopStation's string resources are the storefront's texts, so they are content, not under Nop Station;
            //home page sliders, carousels and product tabs are content, not plugin configuration.
            //each lands right after Menus, so the texts go first to end up last: Menus, sliders.., texts, Message templates
            MoveMenuItemAfter(root, "/NopStationCore/LocaleResource", "Menus");
            MoveMenuItemAfter(root, "/ProductTab/List", "Menus", takeGroupTitle: true);
            MoveMenuItemAfter(root, "/OCarousel/List", "Menus");
            MoveMenuItemAfter(root, "/AnywhereSlider/List", "Menus");

            //once items have moved out, plugin groups left with a single page become that page;
            //plugins hang their sections off the root (Nop Station) or off "Third party plugins"
            var coreSections = _baseRootMenuItem.ChildNodes.Select(node => node.SystemName).ToHashSet();
            foreach (var section in root.ChildNodes.Where(node => !coreSections.Contains(node.SystemName) || node.SystemName == "Third party plugins"))
                CollapseSingleItemGroups(section);

            //whatever is still in the Nop Station section is super-administrator only (the pages are guarded by SuperAdminPluginPagesFilter)
            static bool hasUrl(AdminMenuItem node, string part) =>
                node.Url?.Contains(part, StringComparison.OrdinalIgnoreCase) == true || node.ChildNodes.Any(child => hasUrl(child, part));
            //found by its licence link: "/NopStationCore/" would also match the string resources moved to Content management
            foreach (var section in root.ChildNodes.Where(node => hasUrl(node, "/NopStationLicense/")))
                RestrictMenuItems(section, StandardPermission.Configuration.MANAGE_PLUGIN_AND_WIDGET_LISTS);
        }

        //product tabs are not used yet; hidden, not removed - drop the suffix to bring them back
        HideMenuItems(root, "/ProductTab/List");

        async ValueTask<bool> authorizePermission(string permissionName) => await _permissionService.AuthorizeAsync(permissionName.Trim());

        async Task checkPermissions(AdminMenuItem menuItem, AdminMenuItem rootItem = null)
        {
            if (menuItem.Visible)
            {
                var permissions = (menuItem.PermissionNames.Any() ? menuItem.PermissionNames : (rootItem?.PermissionNames ?? new List<string>())).Distinct().Where(p => !string.IsNullOrEmpty(p)).ToList();

                if (permissions.Any())
                    menuItem.Visible = menuItem.ChildNodes.Any() ? await permissions.AnyAwaitAsync(authorizePermission) : await permissions.AllAwaitAsync(authorizePermission);
            }

            foreach (var childNode in menuItem.ChildNodes)
                await checkPermissions(childNode, menuItem);
        }

        await checkPermissions(root);

        if (showHidden)
            return root;

        void checkVisible(AdminMenuItem menuItem)
        {
            if (!menuItem.ChildNodes.Any())
            {
                menuItem.Visible = menuItem.Visible && !string.IsNullOrEmpty(menuItem.Url);

                return;
            }

            foreach (var childNode in menuItem.ChildNodes)
                checkVisible(childNode);

            menuItem.Visible = menuItem.ChildNodes.Any(n => n.Visible);
        }

        checkVisible(root);

        return root;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the root node
    /// </summary>
    /// <param name="showHidden">A value indicating whether to show hidden records (Visible == false)</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the root menu item
    /// </returns>
    public virtual async Task<AdminMenuItem> GetRootNodeAsync(bool showHidden = false)
    {
        if (_rootItem != null)
            return _rootItem;

        _rootItem = await LoadMenuAsync(showHidden);

        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext ?? throw new ArgumentNullException(nameof(_actionContextAccessor.ActionContext)));

        void transformUrl(AdminMenuItem node)
        {
            if (node.Url?.StartsWith("~/", StringComparison.Ordinal) ?? false)
                node.Url = urlHelper.Content(node.Url);

            foreach (var childNode in node.ChildNodes)
                transformUrl(childNode);
        }

        transformUrl(_rootItem);

        return _rootItem;
    }

    /// <summary>
    /// Generates an admin menu item URL 
    /// </summary>
    /// <param name="controllerName">The name of the controller</param>
    /// <param name="actionName">The name of the action method</param>
    /// <returns>Menu item URL</returns>
    public virtual string GetMenuItemUrl(string controllerName, string actionName)
    {
        if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(actionName))
            return null;

        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext ?? throw new ArgumentNullException(nameof(_actionContextAccessor.ActionContext)));

        return urlHelper.Action(actionName, controllerName, new RouteValueDictionary { { "area", AreaNames.ADMIN } }, null, null);
    }

    #endregion
}