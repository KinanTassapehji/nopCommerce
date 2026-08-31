using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Misc.Core.Services;

public interface INopStationCustomerService
{
	Task<IPagedList<Customer>> GetCustomersAsync(string q = null, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

	Task<string> FormatCustomerNameAsync(Customer customer);
}
