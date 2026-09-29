using System.Security.Cryptography;
using Nop.Core.Domain.Orders;
using Nop.Data;

namespace Nop.Services.Orders;

/// <summary>
/// Custom number formatter
/// </summary>
public partial class CustomNumberFormatter : ICustomNumberFormatter
{
    #region Fields

    protected readonly IRepository<Order> _orderRepository;
    protected readonly OrderSettings _orderSettings;

    #endregion

    #region Ctor

    public CustomNumberFormatter(IRepository<Order> orderRepository,
        OrderSettings orderSettings)
    {
        _orderRepository = orderRepository;
        _orderSettings = orderSettings;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Replace the {CODE} token of a mask with a random 6-digit code no other order has yet
    /// </summary>
    /// <param name="mask">Mask with the other tokens already replaced</param>
    /// <returns>Custom number</returns>
    /// <remarks>
    /// A sequential number tells a customer how many orders the store has had (and, from two of
    /// their own, how many a week), so the number they see can be random. The internal Id stays
    /// sequential for admins.
    /// </remarks>
    protected virtual string ReplaceRandomCode(string mask)
    {
        //one in a million to collide per attempt; the fallback only guards against a store
        //that has used up nearly every code
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var customNumber = mask.Replace("{CODE}", RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6"));
            if (!_orderRepository.Table.Any(order => order.CustomOrderNumber == customNumber))
                return customNumber;
        }

        return mask.Replace("{CODE}", RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8"));
    }

    #endregion

    #region Methods

    /// <summary>
    /// Generate return request custom number
    /// </summary>
    /// <param name="returnRequest">Return request</param>
    /// <returns>Custom number</returns>
    public virtual string GenerateReturnRequestCustomNumber(ReturnRequest returnRequest)
    {
        string customNumber;

        if (string.IsNullOrEmpty(_orderSettings.ReturnRequestNumberMask))
        {
            customNumber = returnRequest.Id.ToString();
        }
        else
        {
            customNumber = _orderSettings.ReturnRequestNumberMask
                .Replace("{ID}", returnRequest.Id.ToString())
                .Replace("{YYYY}", returnRequest.CreatedOnUtc.ToString("yyyy"))
                .Replace("{YY}", returnRequest.CreatedOnUtc.ToString("yy"))
                .Replace("{MM}", returnRequest.CreatedOnUtc.ToString("MM"))
                .Replace("{DD}", returnRequest.CreatedOnUtc.ToString("dd"));

            ////if you need to use the format for the ID with leading zeros, use the following code instead of the previous one.
            ////mask for Id example {#:00000000}
            //var rgx = new Regex(@"{#:\d+}");
            //var match = rgx.Match(customNumber);
            //var maskForReplase = match.Value;
            //
            //rgx = new Regex(@"\d+");
            //match = rgx.Match(maskForReplase);
            //
            //var formatValue = match.Value;
            //if(!string.IsNullOrEmpty(formatValue) && !string.IsNullOrEmpty(maskForReplase))
            //    customNumber = customNumber.Replace(maskForReplase, returnRequest.Id.ToString(formatValue));
            //else
            //    customNumber = customNumber.Insert(0, $"{returnRequest.Id}-");
        }

        return customNumber;
    }

    /// <summary>
    /// Generate order custom number
    /// </summary>
    /// <param name="order">Order</param>
    /// <returns>Custom number</returns>
    public virtual string GenerateOrderCustomNumber(Order order)
    {
        if (string.IsNullOrEmpty(_orderSettings.CustomOrderNumberMask))
            return order.Id.ToString();

        var customNumber = _orderSettings.CustomOrderNumberMask
            .Replace("{ID}", order.Id.ToString())
            .Replace("{YYYY}", order.CreatedOnUtc.ToString("yyyy"))
            .Replace("{YY}", order.CreatedOnUtc.ToString("yy"))
            .Replace("{MM}", order.CreatedOnUtc.ToString("MM"))
            .Replace("{DD}", order.CreatedOnUtc.ToString("dd")).Trim();

        if (customNumber.Contains("{CODE}"))
            customNumber = ReplaceRandomCode(customNumber);

        ////if you need to use the format for the ID with leading zeros, use the following code instead of the previous one.
        ////mask for Id example {#:00000000}
        //var rgx = new Regex(@"{#:\d+}");
        //var match = rgx.Match(customNumber);
        //var maskForReplase = match.Value;

        //rgx = new Regex(@"\d+");
        //match = rgx.Match(maskForReplase);

        //var formatValue = match.Value;
        //if (!string.IsNullOrEmpty(formatValue) && !string.IsNullOrEmpty(maskForReplase))
        //    customNumber = customNumber.Replace(maskForReplase, order.Id.ToString(formatValue));
        //else
        //    customNumber = customNumber.Insert(0, $"{order.Id}-");

        return customNumber;
    }

    #endregion
}