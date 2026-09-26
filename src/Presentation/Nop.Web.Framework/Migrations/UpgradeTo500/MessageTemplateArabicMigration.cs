using FluentMigrator;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Localization;
using Nop.Services.Messages;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Message templates shipped English-only, so Arabic customers got English emails.
/// Adds an Arabic subject and body to every template as its localized value.
/// A template that already has an Arabic body is left alone, so an admin's own wording survives.
/// </summary>
[NopUpdateMigration("2026-09-25 21:00:00", "5.00", UpdateMigrationType.Data)]
public class MessageTemplateArabicMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();
        var messageTemplateService = EngineContext.Current.Resolve<IMessageTemplateService>();

        //TmTm is ar-SY, Arabia is ar-SA
        var arabic = languageService.GetAllLanguages(showHidden: true)
            .Where(language => language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var template in messageTemplateService.GetAllMessageTemplatesAsync(0).Result)
        {
            if (!Templates.TryGetValue(template.Name, out var translation))
                continue;

            foreach (var language in arabic)
            {
                var body = localizationService.GetLocalizedAsync(template, entity => entity.Body, language.Id, returnDefaultValue: false).Result;
                if (!string.IsNullOrEmpty(body))
                    continue;

                localizedEntityService.SaveLocalizedValueAsync(template, entity => entity.Subject, translation.Subject, language.Id).Wait();
                localizedEntityService.SaveLocalizedValueAsync(template, entity => entity.Body,
                    $"<div dir=\"rtl\" style=\"text-align: right;\">\n{translation.Body}\n</div>", language.Id).Wait();
            }
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }

    private const string Header = """
        <a href="%Store.URL%">%Store.Name%</a>
        <br />
        <br />
        """;

    private const string OrderSummary = """
        رقم الطلب: %Order.OrderNumber%
        <br />
        تاريخ الطلب: %Order.CreatedOn%
        """;

    private const string CustomerOrderSummary = """
        رقم الطلب: %Order.OrderNumber%
        <br />
        تفاصيل الطلب: <a target="_blank" href="%Order.OrderURLForCustomer%">%Order.OrderURLForCustomer%</a>
        <br />
        تاريخ الطلب: %Order.CreatedOn%
        """;

    //ends inside the shippable condition, so the caller continues right after " endif%"
    private const string Addresses = """
        <br />
        <br />
        <br />
        <br />
        عنوان الفوترة
        <br />
        %Order.BillingFirstName% %Order.BillingLastName%
        <br />
        %Order.BillingAddress1%
        <br />
        %Order.BillingAddress2%
        <br />
        %Order.BillingCity% %Order.BillingZipPostalCode%
        <br />
        %Order.BillingStateProvince% %Order.BillingCountry%
        <br />
        <br />
        <br />
        <br />
        %if (%Order.Shippable%) عنوان الشحن
        <br />
        %Order.ShippingFirstName% %Order.ShippingLastName%
        <br />
        %Order.ShippingAddress1%
        <br />
        %Order.ShippingAddress2%
        <br />
        %Order.ShippingCity% %Order.ShippingZipPostalCode%
        <br />
        %Order.ShippingStateProvince% %Order.ShippingCountry%
        <br />
        <br />
        طريقة الشحن: %Order.ShippingMethod%
        <br />
        <br />
         endif%
        """;

    private const string QuantityBelow = $"""
        <p>
        {Header}
        انخفضت كمية المنتج %Product.Name% (المعرّف: %Product.ID%).
        <br />
        <br />
        الكمية المتوفرة: %Product.StockQuantity%
        <br />
        </p>
        """;

    private const string AttributeCombinationQuantityBelow = $"""
        <p>
        {Header}
        انخفضت كمية المنتج %Product.Name% (المعرّف: %Product.ID%).
        <br />
        %AttributeCombination.Formatted%
        <br />
        الكمية المتوفرة: %AttributeCombination.StockQuantity%
        <br />
        </p>
        """;

    private const string ContactUs = """
        <p>
        %ContactUs.Body%
        </p>
        """;

    private static readonly Dictionary<string, (string Subject, string Body)> Templates = new()
    {
        ["Blog.BlogComment"] = ("%Store.Name%. تعليق جديد على المدونة.", $"""
            <p>
            {Header}
            تمت إضافة تعليق جديد على التدوينة "%BlogComment.BlogPostTitle%".
            </p>
            """),

        ["Customer.BackInStock"] = ("%Store.Name%. المنتج متوفر من جديد", $"""
            <p>
            {Header}
            مرحباً %Customer.FullName%،
            <br />
            المنتج <a target="_blank" href="%BackInStockSubscription.ProductUrl%">%BackInStockSubscription.ProductName%</a> أصبح متوفراً الآن.
            </p>
            """),

        ["Customer.EmailRevalidationMessage"] = ("%Store.Name%. تأكيد البريد الإلكتروني", $"""
            <p>
            {Header}
            مرحباً %Customer.FullName%!
            <br />
            لتأكيد عنوان بريدك الإلكتروني الجديد <a href="%Customer.EmailRevalidationURL%">اضغط هنا</a>.
            <br />
            <br />
            %Store.Name%
            </p>
            """),

        ["Customer.EmailValidationMessage"] = ("%Store.Name%. تأكيد البريد الإلكتروني", $"""
            {Header}
            لتفعيل حسابك <a href="%Customer.AccountActivationURL%">اضغط هنا</a>.
            <br />
            <br />
            %Store.Name%
            """),

        ["Customer.FailedLoginAttempt"] = ("%Store.Name%. محاولة تسجيل دخول فاشلة", """
            <p>
            وصلك هذا الإشعار لأننا رصدنا محاولة لتسجيل الدخول إلى حسابك ببيانات غير صحيحة في <a href="%Store.URL%">%Store.Name%</a>.
            </p>
            """),

        ["Customer.Gdpr.DeleteRequest"] = ("%Store.Name%. طلب جديد لحذف حساب عميل (GDPR)", """
            طلب العميل %Customer.Email% حذف حسابه. يمكنك مراجعة الطلب من لوحة التحكم.
            """),

        ["Customer.NewOrderNote"] = ("%Store.Name%. ملاحظة جديدة على طلبك", $"""
            <p>
            {Header}
            مرحباً %Customer.FullName%،
            <br />
            تمت إضافة ملاحظة جديدة على طلبك:
            <br />
            "%Order.NewNoteText%".
            <br />
            <a target="_blank" href="%Order.OrderURLForCustomer%">%Order.OrderURLForCustomer%</a>
            </p>
            """),

        ["Customer.NewPM"] = ("%Store.Name%. وصلتك رسالة خاصة جديدة", $"""
            <p>
            {Header}
            وصلتك رسالة خاصة جديدة.
            </p>
            """),

        ["Customer.PasswordRecovery"] = ("%Store.Name%. استعادة كلمة المرور", $"""
            {Header}
            لتغيير كلمة المرور <a href="%Customer.PasswordRecoveryURL%">اضغط هنا</a>.
            <br />
            <br />
            %Store.Name%
            """),

        ["Customer.WelcomeMessage"] = ("أهلاً بك في %Store.Name%", """
            أهلاً بك في <a href="%Store.URL%"> %Store.Name%</a>.
            <br />
            <br />
            يمكنك الآن الاستفادة من الخدمات التي نقدمها لك، ومنها:
            <br />
            <br />
            سلة دائمة - تبقى المنتجات التي تضيفها إلى سلتك محفوظة فيها حتى تحذفها أو تُتم شراءها.
            <br />
            دفتر العناوين - يمكننا توصيل مشترياتك إلى عنوان غير عنوانك، وهذا مثالي لإرسال الهدايا مباشرة إلى أصحابها.
            <br />
            سجل الطلبات - اطّلع على جميع مشترياتك السابقة لدينا.
            <br />
            تقييمات المنتجات - شارك رأيك في المنتجات مع عملائنا الآخرين.
            <br />
            <br />
            للمساعدة في أي من خدماتنا الإلكترونية، يرجى مراسلتنا على: <a href="mailto:%Store.Email%">%Store.Email%</a>.
            <br />
            <br />
            ملاحظة: أُدخل عنوان البريد الإلكتروني هذا في صفحة التسجيل لدينا. إذا كان هذا بريدك ولم تسجّل في موقعنا، يرجى مراسلتنا على <a href="mailto:%Store.Email%">%Store.Email%</a>.
            """),

        ["Forums.NewForumPost"] = ("%Store.Name%. إشعار بمشاركة جديدة.", $"""
            <p>
            {Header}
            أُضيفت مشاركة جديدة في الموضوع <a href="%Forums.TopicURL%">"%Forums.TopicName%"</a> في منتدى <a href="%Forums.ForumURL%">"%Forums.ForumName%"</a>.
            <br />
            <br />
            <a href="%Forums.TopicURL%">اضغط هنا</a> لمزيد من التفاصيل.
            <br />
            <br />
            كاتب المشاركة: %Forums.PostAuthor%
            <br />
            نص المشاركة: %Forums.PostBody%
            </p>
            """),

        ["Forums.NewForumTopic"] = ("%Store.Name%. إشعار بموضوع جديد.", $"""
            <p>
            {Header}
            أُنشئ موضوع جديد <a href="%Forums.TopicURL%">"%Forums.TopicName%"</a> في منتدى <a href="%Forums.ForumURL%">"%Forums.ForumName%"</a>.
            <br />
            <br />
            <a href="%Forums.TopicURL%">اضغط هنا</a> لمزيد من التفاصيل.
            </p>
            """),

        ["NewCustomer.Notification"] = ("%Store.Name%. تسجيل عميل جديد", $"""
            <p>
            {Header}
            سجّل عميل جديد في متجرك. هذه بياناته:
            <br />
            الاسم الكامل: %Customer.FullName%
            <br />
            البريد الإلكتروني: %Customer.Email%
            </p>
            """),

        ["NewReturnRequest.CustomerNotification"] = ("%Store.Name%. طلب إرجاع جديد.", $"""
            <p>
            {Header}
            مرحباً %Customer.FullName%!
            <br />
            لقد قدّمت للتو طلب إرجاع جديداً. هذه تفاصيله:
            <br />
            رقم طلب الإرجاع: %ReturnRequest.CustomNumber%
            <br />
            المنتج: %ReturnRequest.Product.Quantity% × %ReturnRequest.Product.Name%
            <br />
            سبب الإرجاع: %ReturnRequest.Reason%
            <br />
            الإجراء المطلوب: %ReturnRequest.RequestedAction%
            <br />
            ملاحظات العميل:
            <br />
            %ReturnRequest.CustomerComment%
            </p>
            """),

        ["NewReturnRequest.StoreOwnerNotification"] = ("%Store.Name%. طلب إرجاع جديد.", $"""
            <p>
            {Header}
            قدّم %Customer.FullName% للتو طلب إرجاع جديداً. هذه تفاصيله:
            <br />
            رقم طلب الإرجاع: %ReturnRequest.CustomNumber%
            <br />
            المنتج: %ReturnRequest.Product.Quantity% × %ReturnRequest.Product.Name%
            <br />
            سبب الإرجاع: %ReturnRequest.Reason%
            <br />
            الإجراء المطلوب: %ReturnRequest.RequestedAction%
            <br />
            ملاحظات العميل:
            <br />
            %ReturnRequest.CustomerComment%
            </p>
            """),

        ["News.NewsComment"] = ("%Store.Name%. تعليق جديد على خبر.", $"""
            <p>
            {Header}
            تمت إضافة تعليق جديد على الخبر "%NewsComment.NewsTitle%".
            </p>
            """),

        ["NewsLetterSubscription.ActivationMessage"] = ("%Store.Name%. تفعيل الاشتراك في النشرة البريدية.", """
            <p>
            <a href="%NewsLetterSubscription.ActivationUrl%">اضغط هنا لتأكيد اشتراكك في نشرتنا البريدية.</a>
            </p>
            <p>
            إذا وصلتك هذه الرسالة عن طريق الخطأ، يمكنك حذفها ببساطة.
            </p>
            """),

        ["NewsLetterSubscription.DeactivationMessage"] = ("%Store.Name%. إلغاء الاشتراك في النشرة البريدية.", """
            <p>
            <a href="%NewsLetterSubscription.DeactivationUrl%">اضغط هنا لإلغاء اشتراكك في نشرتنا البريدية.</a>
            </p>
            <p>
            إذا وصلتك هذه الرسالة عن طريق الخطأ، يمكنك حذفها ببساطة.
            </p>
            """),

        ["NewVATSubmitted.StoreOwnerNotification"] = ("%Store.Name%. تم تقديم رقم ضريبي جديد.", $"""
            <p>
            {Header}
            قدّم %Customer.FullName% (%Customer.Email%) للتو رقماً ضريبياً جديداً. هذه تفاصيله:
            <br />
            الرقم الضريبي: %Customer.VatNumber%
            <br />
            حالة الرقم الضريبي: %Customer.VatNumberStatus%
            <br />
            الاسم المُستلَم: %VatValidationResult.Name%
            <br />
            العنوان المُستلَم: %VatValidationResult.Address%
            </p>
            """),

        ["OrderCancelled.CustomerNotification"] = ("%Store.Name%. تم إلغاء طلبك", $"""
            <p>
            {Header}
            مرحباً %Order.CustomerFullName%،
            <br />
            تم إلغاء طلبك. هذا ملخص الطلب:
            <br />
            <br />
            {CustomerOrderSummary}
            {Addresses} %Order.Product(s)%
            </p>
            """),

        ["OrderCancelled.StoreOwnerNotification"] = ("%Store.Name%. تم إلغاء الطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            ألغى العميل الطلب رقم %Order.OrderNumber%.
            <br />
            العميل: %Order.CustomerFullName%
            <br />
            <br />
            <br />
            {OrderSummary}
            <br />
            <br />
            %Order.Product(s)%
            </p>
            """),

        ["OrderCancelled.VendorNotification"] = ("%Store.Name%. تم إلغاء الطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            تم إلغاء الطلب رقم %Order.OrderNumber%.
            <br />
            العميل: %Order.CustomerFullName%
            <br />
            <br />
            <br />
            {OrderSummary}
            <br />
            <br />
            %Order.Product(s)%
            </p>
            """),

        ["OrderCompleted.CustomerNotification"] = ("%Store.Name%. تم إكمال طلبك", $"""
            <p>
            {Header}
            مرحباً %Order.CustomerFullName%،
            <br />
            تم إكمال طلبك. هذا ملخص الطلب:
            <br />
            <br />
            {CustomerOrderSummary}
            {Addresses} %Order.Product(s)%
            </p>
            """),

        ["OrderCompleted.StoreOwnerNotification"] = ("%Store.Name%. تم إكمال الطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            تم للتو إكمال طلب العميل %Order.CustomerFullName%. هذا ملخص الطلب:
            <br />
            <br />
            {OrderSummary}
            {Addresses} %Order.Product(s)%
            </p>
            """),

        ["OrderPaid.AffiliateNotification"] = ("%Store.Name%. تم دفع الطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            تم للتو دفع قيمة الطلب رقم %Order.OrderNumber%.
            <br />
            <br />
            {OrderSummary}
            <br />
            <br />
            %Order.Product(s)%
            </p>
            """),

        ["OrderPaid.CustomerNotification"] = ("%Store.Name%. تم دفع الطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            مرحباً %Order.CustomerFullName%،
            <br />
            شكراً لتسوقك من <a href="%Store.URL%">%Store.Name%</a>. تم للتو دفع قيمة الطلب رقم %Order.OrderNumber%. هذا ملخص الطلب:
            <br />
            <br />
            {CustomerOrderSummary}
            {Addresses} %Order.Product(s)%
            </p>
            """),

        ["OrderPaid.StoreOwnerNotification"] = ("%Store.Name%. تم دفع الطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            تم للتو دفع قيمة الطلب رقم %Order.OrderNumber%
            <br />
            تاريخ الطلب: %Order.CreatedOn%
            </p>
            """),

        ["OrderPaid.VendorNotification"] = ("%Store.Name%. تم دفع الطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            تم للتو دفع قيمة الطلب رقم %Order.OrderNumber%.
            <br />
            <br />
            {OrderSummary}
            <br />
            <br />
            %Order.Product(s)%
            </p>
            """),

        ["OrderPlaced.AffiliateNotification"] = ("%Store.Name%. طلب جديد", $"""
            <p>
            {Header}
            قدّم %Customer.FullName% (%Customer.Email%) طلباً جديداً للتو.
            <br />
            <br />
            {OrderSummary}
            <br />
            <br />
            %Order.Product(s)%
            </p>
            """),

        ["OrderPlaced.CustomerNotification"] = ("إيصال طلبك من %Store.Name%.", $"""
            <p>
            {Header}
            مرحباً %Order.CustomerFullName%،
            <br />
            شكراً لتسوقك من <a href="%Store.URL%">%Store.Name%</a>. هذا ملخص الطلب:
            <br />
            <br />
            {CustomerOrderSummary}
            {Addresses} %Order.Product(s)%
            </p>
            """),

        ["OrderPlaced.StoreOwnerNotification"] = ("%Store.Name%. إيصال شراء للطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            قدّم %Order.CustomerFullName% (%Order.CustomerEmail%) للتو طلباً جديداً من متجرك. هذا ملخص الطلب:
            <br />
            <br />
            {OrderSummary}
            {Addresses} %Order.Product(s)%
            </p>
            """),

        ["OrderPlaced.VendorNotification"] = ("%Store.Name%. طلب جديد", $"""
            <p>
            {Header}
            قدّم %Customer.FullName% (%Customer.Email%) طلباً جديداً للتو.
            <br />
            <br />
            {OrderSummary}
            <br />
            <br />
            %Order.Product(s)%
            </p>
            """),

        ["OrderProcessing.CustomerNotification"] = ("%Store.Name%. طلبك قيد المعالجة", $"""
            <p>
            {Header}
            مرحباً %Order.CustomerFullName%،
            <br />
            طلبك قيد المعالجة الآن. هذا ملخص الطلب:
            <br />
            <br />
            {CustomerOrderSummary}
            {Addresses} %Order.Product(s)%
            </p>
            """),

        ["OrderRefunded.CustomerNotification"] = ("%Store.Name%. تم استرداد مبلغ الطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            مرحباً %Order.CustomerFullName%،
            <br />
            شكراً لتسوقك من <a href="%Store.URL%">%Store.Name%</a>. تم استرداد مبلغ الطلب رقم %Order.OrderNumber%. قد يستغرق ظهور المبلغ المسترد في حسابك من 7 إلى 14 يوماً.
            <br />
            <br />
            المبلغ المسترد: %Order.AmountRefunded%
            <br />
            <br />
            هذا ملخص الطلب:
            <br />
            <br />
            {CustomerOrderSummary}
            {Addresses} %Order.Product(s)%
            </p>
            """),

        ["OrderRefunded.StoreOwnerNotification"] = ("%Store.Name%. تم استرداد مبلغ الطلب رقم %Order.OrderNumber%", $"""
            <p>
            {Header}
            تم للتو استرداد مبلغ الطلب رقم %Order.OrderNumber%
            <br />
            <br />
            المبلغ المسترد: %Order.AmountRefunded%
            <br />
            <br />
            تاريخ الطلب: %Order.CreatedOn%
            </p>
            """),

        ["Product.ProductReview"] = ("%Store.Name%. تقييم جديد لمنتج.", $"""
            <p>
            {Header}
            أُضيف تقييم جديد للمنتج "%ProductReview.ProductName%".
            </p>
            """),

        ["ProductReview.Reply.CustomerNotification"] = ("%Store.Name%. رد على تقييمك للمنتج.", $"""
            <p>
            {Header}
            مرحباً %Customer.FullName%،
            <br />
            وصلك رد من إدارة المتجر على تقييمك للمنتج "%ProductReview.ProductName%".
            </p>
            """),

        ["QuantityBelow.AttributeCombination.StoreOwnerNotification"] = ("%Store.Name%. تنبيه انخفاض الكمية. %Product.Name%", AttributeCombinationQuantityBelow),
        ["QuantityBelow.AttributeCombination.VendorNotification"] = ("%Store.Name%. تنبيه انخفاض الكمية. %Product.Name%", AttributeCombinationQuantityBelow),
        ["QuantityBelow.StoreOwnerNotification"] = ("%Store.Name%. تنبيه انخفاض الكمية. %Product.Name%", QuantityBelow),
        ["QuantityBelow.VendorNotification"] = ("%Store.Name%. تنبيه انخفاض الكمية. %Product.Name%", QuantityBelow),

        ["ReturnRequestStatusChanged.CustomerNotification"] = ("%Store.Name%. تم تغيير حالة طلب الإرجاع.", $"""
            <p>
            {Header}
            مرحباً %Customer.FullName%،
            <br />
            تم تغيير حالة طلب الإرجاع رقم %ReturnRequest.CustomNumber%.
            </p>
            """),

        ["Service.ContactUs"] = ("%Store.Name%. اتصل بنا", ContactUs),
        ["Service.ContactVendor"] = ("%Store.Name%. اتصل بنا", ContactUs),

        ["Service.EmailAFriend"] = ("%Store.Name%. منتج رشّحه لك صديق", """
            <p>
            <a href="%Store.URL%"> %Store.Name%</a>
            <br />
            <br />
            كان %EmailAFriend.Email% يتسوق في %Store.Name% وأراد مشاركة هذا المنتج معك.
            <br />
            <br />
            <b><a target="_blank" href="%Product.ProductURLForCustomer%">%Product.Name%</a></b>
            <br />
            %Product.ShortDescription%
            <br />
            <br />
            لمزيد من التفاصيل <a target="_blank" href="%Product.ProductURLForCustomer%">اضغط هنا</a>
            <br />
            <br />
            <br />
            %EmailAFriend.PersonalMessage%
            <br />
            <br />
            %Store.Name%
            </p>
            """),

        //"%if (...) جزء من endif%طلبك": the condition swallows the whitespace around it, so the words sit mid-sentence
        ["ShipmentDelivered.CustomerNotification"] = ("تم توصيل %if (!%Order.IsCompletelyDelivered%) جزء من endif%طلبك من %Store.Name%.", $"""
            <p>
            {Header}
            مرحباً %Order.CustomerFullName%،
            <br />
            أخبار سارة! تم توصيل %if (!%Order.IsCompletelyDelivered%) جزء من endif%طلبك.
            <br />
            {CustomerOrderSummary}
            {Addresses} المنتجات التي تم توصيلها:
            <br />
            <br />
            %Shipment.Product(s)%
            </p>
            """),

        ["ShipmentReadyForPickup.CustomerNotification"] = ("أصبح %if (!%Order.IsCompletelyReadyForPickup%) جزء من endif%طلبك من %Store.Name% جاهزاً للاستلام.", $"""
            <p>
            {Header}
            مرحباً %Order.CustomerFullName%،
            <br />
            أخبار سارة! أصبح %if (!%Order.IsCompletelyReadyForPickup%) جزء من endif%طلبك جاهزاً للاستلام.
            <br />
            {CustomerOrderSummary}
            {Addresses} المنتجات الجاهزة للاستلام:
            <br />
            <br />
            %Shipment.Product(s)%
            </p>
            """),

        ["ShipmentSent.CustomerNotification"] = ("تم شحن %if (!%Order.IsCompletelyShipped%) جزء من endif%طلبك من %Store.Name%.", $"""
            <p>
            {Header}
            مرحباً %Order.CustomerFullName%،
            <br />
            أخبار سارة! تم شحن %if (!%Order.IsCompletelyShipped%) جزء من endif%طلبك.
            <br />
            {CustomerOrderSummary}
            {Addresses} المنتجات المشحونة:
            <br />
            <br />
            %Shipment.Product(s)%
            </p>
            """),

        ["VendorAccountApply.StoreOwnerNotification"] = ("%Store.Name%. طلب حساب بائع جديد.", $"""
            <p>
            {Header}
            قدّم %Customer.FullName% (%Customer.Email%) للتو طلباً لفتح حساب بائع. هذه تفاصيله:
            <br />
            اسم البائع: %Vendor.Name%
            <br />
            البريد الإلكتروني للبائع: %Vendor.Email%
            <br />
            <br />
            يمكنك تفعيل الحساب من لوحة التحكم.
            </p>
            """),

        ["VendorInformationChange.StoreOwnerNotification"] = ("%Store.Name%. تعديل بيانات بائع.", $"""
            <p>
            {Header}
            عدّل البائع %Vendor.Name% (%Vendor.Email%) بياناته للتو.
            </p>
            """)
    };
}
