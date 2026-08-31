using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Messages;

namespace NopStation.Plugin.Misc.CustomerReminders.Helpers;

public static class ReminderRuleTokenGroupHelper
{
	public static List<SelectListItem> GetAvailableTokenGroups()
	{
		return new List<SelectListItem>
		{
			// ponytail: gift card, recurring payment and wishlist-to-friend token groups
			// are not part of this fork
			new SelectListItem
			{
				Text = TokenGroupNames.StoreTokens,
				Value = TokenGroupNames.StoreTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.CustomerTokens,
				Value = TokenGroupNames.CustomerTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.OrderTokens,
				Value = TokenGroupNames.OrderTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.ShipmentTokens,
				Value = TokenGroupNames.ShipmentTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.RefundedOrderTokens,
				Value = TokenGroupNames.RefundedOrderTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.OrderNoteTokens,
				Value = TokenGroupNames.OrderNoteTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.SubscriptionTokens,
				Value = TokenGroupNames.SubscriptionTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.ProductTokens,
				Value = TokenGroupNames.ProductTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.ReturnRequestTokens,
				Value = TokenGroupNames.ReturnRequestTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.ForumTokens,
				Value = TokenGroupNames.ForumTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.ForumTopicTokens,
				Value = TokenGroupNames.ForumTopicTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.ForumPostTokens,
				Value = TokenGroupNames.ForumPostTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.PrivateMessageTokens,
				Value = TokenGroupNames.PrivateMessageTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.VendorTokens,
				Value = TokenGroupNames.VendorTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.ProductReviewTokens,
				Value = TokenGroupNames.ProductReviewTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.AttributeCombinationTokens,
				Value = TokenGroupNames.AttributeCombinationTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.BlogCommentTokens,
				Value = TokenGroupNames.BlogCommentTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.NewsCommentTokens,
				Value = TokenGroupNames.NewsCommentTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.EmailAFriendTokens,
				Value = TokenGroupNames.EmailAFriendTokens
			},
			new SelectListItem
			{
				Text = TokenGroupNames.VatValidation,
				Value = TokenGroupNames.VatValidation
			},
			new SelectListItem
			{
				Text = TokenGroupNames.ContactUs,
				Value = TokenGroupNames.ContactUs
			},
			new SelectListItem
			{
				Text = TokenGroupNames.ContactVendor,
				Value = TokenGroupNames.ContactVendor
			}
		};
	}

	public static List<string> ParseTokenGroups(string tokenGroups)
	{
		if (string.IsNullOrWhiteSpace(tokenGroups))
		{
			return new List<string>();
		}
		return (from x in tokenGroups.Split(',', StringSplitOptions.RemoveEmptyEntries)
			select x.Trim()).ToList();
	}

	public static string JoinTokenGroups(IList<string> tokenGroupNames)
	{
		if (tokenGroupNames == null || !tokenGroupNames.Any())
		{
			return string.Empty;
		}
		return string.Join(",", tokenGroupNames);
	}
}
