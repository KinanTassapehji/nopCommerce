using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.CustomerReminders.Domains;

public class ReminderReport : BaseEntity
{
	public int? ReminderId { get; set; }

	public string ReminderName { get; set; }

	public int? CustomerId { get; set; }

	public string CustomerName { get; set; }

	public string CustomerEmail { get; set; }

	public int StoreId { get; set; }

	public string StoreName { get; set; }

	public DateTime CreatedOnUtc { get; set; }

	public bool IsMessageSent { get; set; }
}
