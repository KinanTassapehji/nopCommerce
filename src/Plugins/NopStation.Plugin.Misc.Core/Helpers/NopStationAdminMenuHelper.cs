using System.Linq;
using Nop.Web.Framework.Menu;

namespace NopStation.Plugin.Misc.Core.Helpers;

public static class NopStationAdminMenuHelper
{
	private static bool Insert(AdminMenuItem currentMenuItem, string itemSystemName, AdminMenuItem newMenuItem)
	{
		int num = 0;
		bool flag = false;
		foreach (AdminMenuItem item in currentMenuItem.ChildNodes.ToList())
		{
			if (!item.SystemName.Equals(itemSystemName))
			{
				num++;
				continue;
			}
			item.ChildNodes.Add(newMenuItem);
			flag = true;
			break;
		}
		if (flag)
		{
			return true;
		}
		foreach (AdminMenuItem childNode in currentMenuItem.ChildNodes)
		{
			flag = Insert(childNode, itemSystemName, newMenuItem);
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	public static bool InsertInside(this AdminMenuItem adminMenuItem, string itemSystemName, AdminMenuItem newMenuItem)
	{
		return Insert(adminMenuItem, itemSystemName, newMenuItem);
	}
}
