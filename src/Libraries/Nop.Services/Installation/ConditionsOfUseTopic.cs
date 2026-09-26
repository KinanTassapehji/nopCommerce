namespace Nop.Services.Installation;

/// <summary>
/// The "ConditionsOfUse" topic copy, Arabic-first with English as its localized value.
/// Shared by the installer and the migration that replaces the old placeholder, so the
/// terms live in one place.
///
/// Plain h2/p/ul markup only: .topic-page .page-body in brand.css styles it (both
/// directions, every width), and an admin can keep editing it in the rich editor.
///
/// ponytail: the facts the store has not settled yet (legal name, phone, delivery
/// areas, return window, warranty) are [bracketed] blanks to fill in from the admin.
/// </summary>
public static class ConditionsOfUseTopic
{
    public const string ArabicTitle = "شروط الاستخدام";

    public const string EnglishTitle = "Terms and Conditions";

    /// <summary>The body the installer used to seed; the migration only replaces this</summary>
    public const string ArabicPlaceholder = "<p>اكتب هنا شروط الاستخدام. يمكنك تعديل هذا النص من لوحة التحكم.</p>";

    public const string ArabicBody =
        "<p>مرحباً بك في متجر تمتم الإلكتروني. تنظّم هذه الشروط استخدامك للموقع وتطبيقه وشراءك منهما، ويُعدّ دخولك إلى المتجر أو إتمامك لأي طلب موافقةً منك عليها، فنرجو قراءتها بعناية.</p>"
        + "<h2>التعريفات</h2><ul>"
        + "<li><strong>تمتم أو «نحن»:</strong> [الاسم القانوني للشركة]، ومقرها دمشق، الجمهورية العربية السورية.</li>"
        + "<li><strong>المتجر:</strong> موقع تمتم الإلكتروني وتطبيقه على الهاتف وجميع صفحاتهما وخدماتهما.</li>"
        + "<li><strong>العميل أو «أنت»:</strong> كل من يتصفح المتجر أو ينشئ حساباً فيه أو يشتري من خلاله.</li>"
        + "<li><strong>المنتجات:</strong> السلع المعروضة للبيع في المتجر.</li>"
        + "</ul>"
        + "<h2>الحساب والتسجيل</h2><ul>"
        + "<li>يجب أن يكون عمرك 18 عاماً فأكثر لإنشاء حساب أو إتمام عملية شراء.</li>"
        + "<li>تلتزم بتقديم بيانات صحيحة ومحدّثة، بما فيها الاسم ورقم الجوال وعنوان التوصيل.</li>"
        + "<li>أنت مسؤول عن سرية كلمة المرور وعن كل ما يتم من خلال حسابك، وعليك إبلاغنا فوراً عند الاشتباه بأي استخدام غير مصرّح به.</li>"
        + "<li>يحق لتمتم تعليق أي حساب أو إلغاؤه عند مخالفة هذه الشروط أو الاشتباه بإساءة الاستخدام.</li>"
        + "</ul>"
        + "<h2>المنتجات والأسعار</h2><ul>"
        + "<li>نحرص على عرض أوصاف المنتجات وصورها بدقة، وقد تختلف الألوان أو التفاصيل اختلافاً طفيفاً عن الصور بحسب إعدادات شاشتك.</li>"
        + "<li>الأسعار معروضة بالليرة السورية، وتظهر رسوم التوصيل منفصلة قبل تأكيد الطلب.</li>"
        + "<li>الأسعار والعروض والتوفّر قابلة للتغيير دون إشعار مسبق، ويُعتمد السعر الظاهر لحظة تأكيد الطلب.</li>"
        + "<li>إذا ظهر سعر خاطئ نتيجة خطأ تقني أو مطبعي، يحق لنا إلغاء الطلب وإعادة أي مبلغ مدفوع إليك كاملاً.</li>"
        + "</ul>"
        + "<h2>الطلبات والدفع</h2><ul>"
        + "<li>يُعدّ الطلب مقبولاً بعد وصول تأكيد منا، ويحق لنا رفض أي طلب أو إلغاؤه عند نفاد الكمية أو تعذّر التحقق من بيانات الطلب أو العنوان.</li>"
        + "<li>يتم الدفع بوسائل الدفع المتاحة عند إتمام الطلب.</li>"
        + "<li>يمكنك إلغاء طلبك من صفحة تفاصيل الطلب ما دام لم يُشحن بعد.</li>"
        + "</ul>"
        + "<h2>التوصيل</h2><ul>"
        + "<li>نوصل الطلبات إلى [المحافظات أو المدن المشمولة بالتوصيل]، إلى العنوان المحدد في الطلب.</li>"
        + "<li>مواعيد التوصيل تقديرية، وقد تتأخر لأسباب خارجة عن إرادتنا، وسنبلغك بأي تأخير.</li>"
        + "<li>عند الاستلام، يُرجى فحص المنتج وتغليفه وإبلاغ مندوب التوصيل أو خدمة العملاء فوراً بأي تلف ظاهر.</li>"
        + "</ul>"
        + "<h2>الإرجاع والاستبدال</h2><ul>"
        + "<li>يحق لك إرجاع المنتج أو استبداله خلال [عدد] أيام من تاريخ الاستلام.</li>"
        + "<li>يُشترط أن يكون المنتج بحالته الأصلية، غير مستخدم، ومع تغليفه وملحقاته وإيصال الشراء.</li>"
        + "<li>إذا كان المنتج معيباً أو مخالفاً للطلب، نتحمل تكاليف إعادته ونستبدله أو نعيد المبلغ كاملاً.</li>"
        + "<li>لا تُقبل إعادة المنتجات القابلة للتلف أو ذات الاستخدام الشخصي بعد فتح تغليفها، ما لم تكن معيبة.</li>"
        + "</ul>"
        + "<h2>الضمان</h2>"
        + "<p>تخضع المنتجات لضمان [مدة الضمان] أو لضمان الوكيل حسب ما هو مذكور في صفحة كل منتج، ولا يشمل الضمان الأعطال الناتجة عن سوء الاستخدام أو التعديل على المنتج أو تذبذب التيار الكهربائي.</p>"
        + "<h2>الاستخدام المقبول</h2>"
        + "<p>تلتزم باستخدام المتجر لأغراض مشروعة فقط، ويُحظر عليك:</p><ul>"
        + "<li>انتحال شخصية الغير أو تقديم بيانات مضللة.</li>"
        + "<li>محاولة الوصول غير المصرّح به إلى المتجر أو أنظمته أو حسابات العملاء الآخرين.</li>"
        + "<li>نشر أي محتوى مسيء أو مخالف للقوانين أو الآداب العامة في التقييمات أو المراسلات.</li>"
        + "<li>استخدام أدوات آلية لجمع البيانات من المتجر أو التأثير على أدائه.</li>"
        + "</ul>"
        + "<h2>الملكية الفكرية</h2>"
        + "<p>جميع محتويات المتجر من نصوص وصور وشعارات وتصاميم، بما فيها اسم «تمتم» وشعاره، مملوكة لتمتم أو لأصحابها المرخِّصين، ولا يجوز نسخها أو إعادة نشرها أو استخدامها تجارياً دون موافقة كتابية مسبقة.</p>"
        + "<h2>حدود المسؤولية</h2>"
        + "<p>نسعى لإبقاء المتجر متاحاً ودقيقاً، لكننا لا نضمن خلوّه من الانقطاع أو الأخطاء. وفي حدود ما يسمح به القانون، لا تتجاوز مسؤولية تمتم تجاه أي طلب قيمة المنتج المشترى، ولا تتحمل تمتم الأضرار غير المباشرة الناتجة عن استخدام المتجر أو تعذّر استخدامه.</p>"
        + "<h2>الخصوصية وحماية البيانات</h2>"
        + "<p>نجمع بياناتك الشخصية ونعالجها لتنفيذ طلباتك وتقديم خدماتنا فقط، ولا نبيعها لأي طرف ثالث، ونشاركها فقط مع من يلزم لإيصال طلبك، وفقاً لسياسة الخصوصية المنشورة في المتجر.</p>"
        + "<h2>تعديل الشروط</h2>"
        + "<p>يحق لتمتم تعديل هذه الشروط في أي وقت، وتسري التعديلات من تاريخ نشرها على هذه الصفحة. ويُعدّ استمرارك في استخدام المتجر بعد النشر موافقةً على الشروط المعدّلة، ولا تسري التعديلات على الطلبات المؤكدة قبل نشرها.</p>"
        + "<h2>القانون الواجب التطبيق</h2>"
        + "<p>تخضع هذه الشروط وتُفسَّر وفقاً للقوانين النافذة في الجمهورية العربية السورية، ومنها قوانين حماية المستهلك، وتختص محاكم دمشق بالنظر في أي نزاع ينشأ عنها، مع حرصنا على حل أي خلاف ودياً أولاً.</p>"
        + "<h2>تواصل معنا</h2>"
        + "<p>لأي استفسار حول هذه الشروط، يسعدنا تواصلك معنا على الرقم <strong>[رقم الهاتف]</strong> أو من خلال <a href=\"/contactus\">صفحة اتصل بنا</a>.</p>";

    public const string EnglishBody =
        "<p>Welcome to the TmTm online store. These terms govern your use of our website and app and your purchases through them. By browsing the store or placing an order you agree to them, so please read them carefully.</p>"
        + "<h2>Definitions</h2><ul>"
        + "<li><strong>TmTm, \"we\" or \"us\":</strong> [company legal name], based in Damascus, Syrian Arab Republic.</li>"
        + "<li><strong>The Store:</strong> the TmTm website and mobile app, and all of their pages and services.</li>"
        + "<li><strong>The Customer or \"you\":</strong> anyone who browses the store, creates an account on it or buys through it.</li>"
        + "<li><strong>Products:</strong> the goods offered for sale in the store.</li>"
        + "</ul>"
        + "<h2>Account and Registration</h2><ul>"
        + "<li>You must be 18 years of age or older to create an account or complete a purchase.</li>"
        + "<li>You agree to provide accurate, up-to-date information, including your name, mobile number and delivery address.</li>"
        + "<li>You are responsible for keeping your password confidential and for all activity under your account, and must notify us immediately of any suspected unauthorised use.</li>"
        + "<li>TmTm may suspend or close any account that breaches these terms or is suspected of misuse.</li>"
        + "</ul>"
        + "<h2>Products and Prices</h2><ul>"
        + "<li>We take care to describe and picture our products accurately; colours and details may differ slightly from the images depending on your screen.</li>"
        + "<li>Prices are shown in Syrian Pounds. Delivery charges are shown separately before you confirm your order.</li>"
        + "<li>Prices, offers and availability may change without prior notice. The price shown when you confirm your order is the one that applies.</li>"
        + "<li>If a price is displayed in error due to a technical or typographical mistake, we may cancel the order and refund any amount paid in full.</li>"
        + "</ul>"
        + "<h2>Orders and Payment</h2><ul>"
        + "<li>An order is accepted once you receive our confirmation. We may decline or cancel an order if stock runs out or the order or address details cannot be verified.</li>"
        + "<li>Payment is made using the methods available at checkout.</li>"
        + "<li>You can cancel your order from the order details page as long as it has not been shipped.</li>"
        + "</ul>"
        + "<h2>Delivery</h2><ul>"
        + "<li>We deliver to [governorates or cities covered], to the address given in the order.</li>"
        + "<li>Delivery dates are estimates and may be affected by circumstances beyond our control; we will let you know of any delay.</li>"
        + "<li>On delivery, please inspect the product and its packaging and report any visible damage to the courier or customer service straight away.</li>"
        + "</ul>"
        + "<h2>Returns and Exchanges</h2><ul>"
        + "<li>You may return or exchange a product within [number] days of receiving it.</li>"
        + "<li>The product must be in its original condition, unused, with its packaging, accessories and receipt.</li>"
        + "<li>If a product is defective or not as ordered, we cover the return costs and replace it or refund the full amount.</li>"
        + "<li>Perishable and personal-use products cannot be returned once opened, unless they are defective.</li>"
        + "</ul>"
        + "<h2>Warranty</h2>"
        + "<p>Products carry a [warranty period] warranty or the distributor's warranty, as stated on each product page. The warranty does not cover faults caused by misuse, modification of the product, or power fluctuations.</p>"
        + "<h2>Acceptable Use</h2>"
        + "<p>You agree to use the store for lawful purposes only. You must not:</p><ul>"
        + "<li>Impersonate another person or provide misleading information.</li>"
        + "<li>Attempt unauthorised access to the store, its systems or other customers' accounts.</li>"
        + "<li>Post offensive or unlawful content in reviews or correspondence.</li>"
        + "<li>Use automated tools to harvest data from the store or interfere with its operation.</li>"
        + "</ul>"
        + "<h2>Intellectual Property</h2>"
        + "<p>All store content, including text, images, logos and designs, and the TmTm name and logo, is owned by TmTm or its licensors and may not be copied, republished or used commercially without prior written consent.</p>"
        + "<h2>Limitation of Liability</h2>"
        + "<p>We strive to keep the store available and accurate but do not guarantee it will be uninterrupted or error-free. To the extent permitted by law, TmTm's liability for any order does not exceed the value of the product purchased, and TmTm is not liable for indirect damages arising from the use of, or inability to use, the store.</p>"
        + "<h2>Privacy and Data Protection</h2>"
        + "<p>We collect and process your personal data solely to fulfil your orders and provide our services. We never sell it, and share it only with those needed to deliver your order, in accordance with the Privacy Policy published in the store.</p>"
        + "<h2>Changes to These Terms</h2>"
        + "<p>TmTm may amend these terms at any time. Changes take effect when published on this page, and continuing to use the store afterwards means you accept them. Changes do not apply to orders confirmed before they were published.</p>"
        + "<h2>Governing Law</h2>"
        + "<p>These terms are governed by and interpreted under the laws in force in the Syrian Arab Republic, including its consumer protection laws. The courts of Damascus have jurisdiction over any dispute arising from them, though we will always try to resolve a disagreement amicably first.</p>"
        + "<h2>Contact Us</h2>"
        + "<p>For any questions about these terms, call us on <strong>[phone number]</strong> or reach us through the <a href=\"/contactus\">Contact Us page</a>.</p>";
}