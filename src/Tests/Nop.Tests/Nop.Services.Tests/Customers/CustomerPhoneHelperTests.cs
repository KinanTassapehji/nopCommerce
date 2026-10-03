using FluentAssertions;
using Nop.Services.Customers;
using NUnit.Framework;

namespace Nop.Tests.Nop.Services.Tests.Customers;

[TestFixture]
public class CustomerPhoneHelperTests
{
    [TestCase("0933 123 456", "SY")]
    [TestCase("0933123456", null)]
    [TestCase("+963 933 123 456", "SY")]
    [TestCase("00963933123456", "SY")]
    [TestCase("+963933123456", "DE")]
    [TestCase("٠٩٣٣١٢٣٤٥٦", "SY")]
    public void ToE164ReadsEveryWayASyrianNumberIsTyped(string phone, string region)
    {
        CustomerPhoneHelper.ToE164(phone, region).Should().Be("+963933123456");
    }

    [Test]
    public void ToE164ReadsALocalNumberInThePickedCountry()
    {
        CustomerPhoneHelper.ToE164("01701234567", "DE").Should().Be("+491701234567");
        CustomerPhoneHelper.ToE164("050 123 4567", "ae").Should().Be("+971501234567");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("12")]
    [TestCase("not a phone")]
    [TestCase("admin@tmtm.com")]
    public void ToE164RejectsWhatIsNotAPhoneNumber(string phone)
    {
        CustomerPhoneHelper.ToE164(phone, "SY").Should().BeNull();
    }

    [Test]
    public void SplitGivesTheFieldBackItsCountryAndLocalNumber()
    {
        var (region, national) = CustomerPhoneHelper.Split("+491701234567");
        region.Should().Be("DE");
        CustomerPhoneHelper.ToE164(national, region).Should().Be("+491701234567");
        CustomerPhoneHelper.Split("0933123456").Region.Should().Be("SY");
        CustomerPhoneHelper.Split(null).Should().Be(("SY", null));
    }
}