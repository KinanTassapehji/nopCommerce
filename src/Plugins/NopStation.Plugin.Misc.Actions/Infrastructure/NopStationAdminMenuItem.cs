using System;
using Nop.Web.Framework.Menu;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class NopStationAdminMenuItem : AdminMenuItem
{
	public int DisplayOrer { get; set; }

	public NopStationAdminMenuItem()
	{
		base.SystemName = Guid.NewGuid().ToString();
	}
}
