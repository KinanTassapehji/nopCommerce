using System;
using System.Linq;
using Nop.Core;

namespace NopStation.Plugin.Misc.CustomerReminders.Helpers;

public static class EmailValidationHelper
{
	public static bool AreValidEmails(string bccList)
	{
		if (string.IsNullOrWhiteSpace(bccList))
		{
			return true;
		}
		return (from e in bccList.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries)
			select e.Trim() into e
			where !string.IsNullOrEmpty(e)
			select e).All(CommonHelper.IsValidEmail);
	}
}
