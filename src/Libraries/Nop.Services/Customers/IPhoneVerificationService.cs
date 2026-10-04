using Nop.Core.Domain.Customers;

namespace Nop.Services.Customers;

/// <summary>
/// Confirms that a customer owns a phone number by sending a code to it over WhatsApp
/// </summary>
public partial interface IPhoneVerificationService
{
    /// <summary>
    /// Get the code last sent to a customer
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the code; null when none was sent
    /// </returns>
    Task<PhoneCode> GetCodeAsync(Customer customer);

    /// <summary>
    /// Get whether a number can receive a code
    /// </summary>
    /// <param name="phone">Number, E.164</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains false only when WhatsApp says the number is not on it
    /// </returns>
    Task<bool> CanReceiveCodeAsync(string phone);

    /// <summary>
    /// Send a new code, replacing the previous one
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="phone">Number to send to, E.164</param>
    /// <param name="purpose">What the code confirms</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains how it went
    /// </returns>
    Task<PhoneCodeSendResult> SendCodeAsync(Customer customer, string phone, PhoneVerificationPurpose purpose);

    /// <summary>
    /// Check an entered code; a valid code is used up
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="code">Code as entered</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains how it went
    /// </returns>
    Task<PhoneCodeCheckResult> CheckCodeAsync(Customer customer, string code);

    /// <summary>
    /// Get whether a new account still waits for its number to be confirmed
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains true while it waits
    /// </returns>
    Task<bool> IsActivationPendingAsync(Customer customer);

    /// <summary>
    /// Set whether a new account waits for its number to be confirmed
    /// </summary>
    /// <param name="customer">Customer</param>
    /// <param name="pending">True while it waits</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task SetActivationPendingAsync(Customer customer, bool pending);
}