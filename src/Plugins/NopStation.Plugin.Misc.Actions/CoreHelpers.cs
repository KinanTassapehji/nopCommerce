using System;
using System.Linq;

namespace NopStation.Plugin.Misc.Core;

public class CoreHelpers
{
	public static string RandomString(int length)
	{
		Random random = new Random();
		return new string((from s in Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz0123456789-", length)
			select s[random.Next(s.Length)]).ToArray());
	}
}
