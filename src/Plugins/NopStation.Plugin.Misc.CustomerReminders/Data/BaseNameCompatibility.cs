using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.CustomerReminders.Domains;

namespace NopStation.Plugin.Misc.CustomerReminders.Data;

public class BaseNameCompatibility : INameCompatibility
{
	public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
	{
		{
			typeof(ReminderRule),
			CustomerRemindersDefaults.TableNamePrefix + "ReminderRule"
		},
		{
			typeof(Reminder),
			CustomerRemindersDefaults.TableNamePrefix + "Reminder"
		},
		{
			typeof(ReminderReport),
			CustomerRemindersDefaults.TableNamePrefix + "ReminderReport"
		},
		{
			typeof(ReminderExcludedCustomer),
			CustomerRemindersDefaults.TableNamePrefix + "ReminderExcludedCustomer"
		}
	};

	public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>();
}
