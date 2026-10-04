using FluentAssertions;
using Nop.Services.Customers;
using NUnit.Framework;

namespace Nop.Tests.Nop.Services.Tests.Customers;

[TestFixture]
public class PhoneVerificationServiceTests
{
    private static readonly Guid _customer = Guid.NewGuid();
    private static readonly DateTime _now = new(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);

    private static PhoneCode Sent(string code = "123456", int attempts = 0, int minutesLeft = 5)
    {
        return new PhoneCode
        {
            Hash = PhoneVerificationService.HashCode(_customer, code),
            Phone = "+963944555123",
            Purpose = PhoneVerificationPurpose.Activate,
            ExpiresUtc = _now.AddMinutes(minutesLeft),
            Attempts = attempts
        };
    }

    [TestCase("123456")]
    [TestCase(" 123 456 ")]
    [TestCase("١٢٣٤٥٦")]
    public void CheckAcceptsTheCodeAsCustomersTypeIt(string entered)
    {
        PhoneVerificationService.Check(Sent(), _customer, entered, _now).Should().Be(PhoneCodeCheckResult.Valid);
    }

    [Test]
    public void CheckRefusesAWrongCode()
    {
        PhoneVerificationService.Check(Sent(), _customer, "654321", _now).Should().Be(PhoneCodeCheckResult.Wrong);
        PhoneVerificationService.Check(Sent(), _customer, null, _now).Should().Be(PhoneCodeCheckResult.Wrong);
    }

    [Test]
    public void CheckRefusesAnotherCustomersCode()
    {
        PhoneVerificationService.Check(Sent(), Guid.NewGuid(), "123456", _now).Should().Be(PhoneCodeCheckResult.Wrong);
    }

    [Test]
    public void CheckRefusesAnExpiredCode()
    {
        PhoneVerificationService.Check(Sent(minutesLeft: 0), _customer, "123456", _now).Should().Be(PhoneCodeCheckResult.Expired);
    }

    [Test]
    public void CheckStopsAfterFiveWrongCodesEvenForTheRightOne()
    {
        PhoneVerificationService.Check(Sent(attempts: 5), _customer, "123456", _now).Should().Be(PhoneCodeCheckResult.TooManyAttempts);
    }

    [Test]
    public void CheckRefusesAUsedOrMissingCode()
    {
        var used = Sent();
        used.Hash = null;

        PhoneVerificationService.Check(used, _customer, "123456", _now).Should().Be(PhoneCodeCheckResult.NoCode);
        PhoneVerificationService.Check(null, _customer, "123456", _now).Should().Be(PhoneCodeCheckResult.NoCode);
    }
}