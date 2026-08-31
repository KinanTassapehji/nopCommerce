using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.Core.Domains.SMS;

public class QueuedSms : BaseEntity
{
	public int? CustomerId { get; set; }

	public int StoreId { get; set; }

	public string Body { get; set; }

	public string PhoneNumber { get; set; }

	public int SentTries { get; set; }

	public string Error { get; set; }

	public DateTime CreatedOnUtc { get; set; }

	public DateTime? SentOnUtc { get; set; }

	public string ProviderSystemName { get; set; }

	public string ExternalMessageId { get; set; }
}
