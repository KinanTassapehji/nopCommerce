using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using NopStation.Plugin.Misc.CustomerReminders.Domains.Enums;

namespace NopStation.Plugin.Misc.CustomerReminders.Domains;

public class Reminder : BaseEntity, ISoftDeletedEntity
{
	public string Name { get; set; }

	public int ReminderRuleId { get; set; }

	public bool IsEnabled { get; set; }

	public int StoreId { get; set; }

	public int VendorId { get; set; }

	public int DateGreaterThanIntervalTypeId { get; set; }

	public int DateLowerThanIntervalTypeId { get; set; }

	public int IntervalBetweenMessagesTypeId { get; set; }

	public int DateGreaterThan { get; set; }

	public int DateLowerThan { get; set; }

	public int IntervalBetweenMessages { get; set; }

	public int MessageTemplateId { get; set; }

	public int MaxMessagesPerCustomer { get; set; }

	public DateTime ExecutedOnUtc { get; set; }

	public bool Deleted { get; set; }

	public IntervalType DateGreaterThanIntervalType
	{
		get
		{
			return (IntervalType)DateGreaterThanIntervalTypeId;
		}
		set
		{
			DateGreaterThanIntervalTypeId = (int)value;
		}
	}

	public IntervalType DateLowerThanIntervalType
	{
		get
		{
			return (IntervalType)DateLowerThanIntervalTypeId;
		}
		set
		{
			DateLowerThanIntervalTypeId = (int)value;
		}
	}

	public IntervalType IntervalBetweenMessagesType
	{
		get
		{
			return (IntervalType)IntervalBetweenMessagesTypeId;
		}
		set
		{
			IntervalBetweenMessagesTypeId = (int)value;
		}
	}
}
