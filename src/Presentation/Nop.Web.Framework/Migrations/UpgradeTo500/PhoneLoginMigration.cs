using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Customers log in with their phone number instead of their email, and email becomes optional.
/// nopCommerce's own "usernames" mode does the login part; the username is the phone number in
/// E.164 (+963933123456), which the store sets itself - customers never type a username.
/// Existing phones are rewritten to E.164 and become the username. A number that does not parse,
/// or that more than one account shares, keeps its account on its old username (the email, which
/// still logs in) and is logged so an admin can sort it out.
/// </summary>
[NopUpdateMigration("2026-10-02 12:00:00", "5.00", UpdateMigrationType.Data)]
public class PhoneLoginMigration : MigrationBase
{
    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var settingService = EngineContext.Current.Resolve<ISettingService>();
        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
        var languageService = EngineContext.Current.Resolve<ILanguageService>();
        var customerRepository = EngineContext.Current.Resolve<IRepository<Customer>>();
        var logger = EngineContext.Current.Resolve<ILogger>();

        var customerSettings = settingService.LoadSetting<CustomerSettings>();
        customerSettings.UsernamesEnabled = true;
        customerSettings.AllowUsersToChangeUsernames = false;
        customerSettings.CheckUsernameAvailabilityEnabled = false;
        customerSettings.UsernameValidationEnabled = false;
        customerSettings.PhoneEnabled = true;
        customerSettings.PhoneRequired = true;
        //the settings regex only knows one country's numbers; CustomerPhoneHelper validates every country
        customerSettings.PhoneNumberValidationEnabled = false;
        settingService.SaveSetting(customerSettings);

        var customers = customerRepository.Table
            .Where(customer => !customer.Deleted && ((customer.Phone != null && customer.Phone != string.Empty) || customer.Email != null))
            .ToList();
        var skipped = new List<string>();
        var phoneLogins = new List<Customer>();

        foreach (var group in customers.Where(customer => !string.IsNullOrEmpty(customer.Phone))
            .GroupBy(customer => CustomerPhoneHelper.ToE164(customer.Phone, null)))
        {
            if (group.Key == null)
            {
                skipped.AddRange(group.Select(customer => $"#{customer.Id} {customer.Phone}: not a valid number"));
                continue;
            }

            foreach (var customer in group)
                customer.Phone = group.Key;

            if (group.Count() > 1)
            {
                skipped.Add($"{group.Key}: shared by customers {string.Join(", ", group.Select(customer => $"#{customer.Id}"))}");
                continue;
            }

            group.Single().Username = group.Key;
            phoneLogins.Add(group.Single());
        }

        //everyone else logs in with their email, so that becomes their username: the login cookie
        //carries only the username, and some accounts have none (or a stale one, like the
        //installer's admin@yourstore.com)
        foreach (var customer in customers.Except(phoneLogins).Where(customer => !string.IsNullOrEmpty(customer.Email)))
            customer.Username = customer.Email;

        customerRepository.Update(customers, false);

        if (skipped.Any())
            logger.Warning($"Phone login: these customers keep their old username (their email) until their phone number is fixed:{Environment.NewLine}{string.Join(Environment.NewLine, skipped)}");

        foreach (var language in languageService.GetAllLanguages(showHidden: true))
        {
            var arabic = language.LanguageCulture.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
            localizationService.AddOrUpdateLocaleResource(new Dictionary<string, string>
            {
                ["Account.Login.Fields.Username"] = arabic ? "رقم الهاتف" : "Phone number",
                ["Account.Fields.PhoneCountry"] = arabic ? "رمز الدولة" : "Country code",
                ["Account.Register.Errors.UsernameIsNotProvided"] = arabic ? "رقم الهاتف مطلوب" : "Phone number is required",
                ["Account.Register.Errors.UsernameAlreadyExists"] = arabic ? "رقم الهاتف هذا مسجّل لحساب آخر" : "This phone number is already registered",
                ["Account.EmailUsernameErrors.UsernameAlreadyExists"] = arabic ? "رقم الهاتف هذا مسجّل لحساب آخر" : "This phone number is already registered",
                ["Account.Fields.Phone.NotValid"] = arabic ? "رقم الهاتف غير صحيح" : "Phone number is not valid"
            }, language.Id);
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}