using FluentAssertions;
using Nop.Services.Localization;
using NUnit.Framework;

namespace Nop.Tests.Nop.Services.Tests.Localization;

[TestFixture]
public class LanguageServiceTests : ServiceTest
{
    private ILanguageService _languageService;

    [OneTimeSetUp]
    public void SetUp()
    {
        _languageService = GetService<ILanguageService>();
    }

    [Test]
    public async Task CanGetAllLanguages()
    {
        var languages = await _languageService.GetAllLanguagesAsync();
        languages.Should().NotBeNull();
        languages.Any().Should().BeTrue();
    }

    [Test]
    public async Task CanGetBundledLanguagePack()
    {
        //the store ships an Arabic pack in App_Data/Localization/LanguagePacks, the installer must pick it up
        var languages = await _languageService.GetAllLanguagesAsync();

        //a bundled pack outranks English, so it is the language a new customer gets
        languages.First().LanguageCulture.Should().Be("ar-SY");

        var language = languages.First();
        language.Rtl.Should().BeTrue();

        var resource = await GetService<ILocalizationService>().GetResourceAsync("Account.AccountActivation", language.Id, returnEmptyIfNotFound: true);
        resource.Should().NotBeEmpty();
    }
}