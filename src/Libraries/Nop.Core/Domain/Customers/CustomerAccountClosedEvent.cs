namespace Nop.Core.Domain.Customers;

/// <summary>
/// The customer closed their own account (it is deactivated, not deleted)
/// </summary>
public partial class CustomerAccountClosedEvent
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="customer">customer</param>
    public CustomerAccountClosedEvent(Customer customer)
    {
        Customer = customer;
    }

    /// <summary>
    /// Customer
    /// </summary>
    public Customer Customer
    {
        get;
    }
}