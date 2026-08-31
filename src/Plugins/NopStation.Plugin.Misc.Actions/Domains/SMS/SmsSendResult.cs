using System;
using System.Collections.Generic;

namespace NopStation.Plugin.Misc.Core.Domains.SMS;

public class SmsSendResult
{
	public bool Success { get; set; }

	public string Message { get; set; }

	public Exception Exception { get; set; }

	public string ExternalMessageId { get; set; }

	public DateTime? SentOnUtc { get; set; }

	public IList<string> Errors { get; set; }

	public SmsSendResult()
	{
		Errors = new List<string>();
	}

	public void AddError(string error)
	{
		Errors.Add(error);
	}
}
