using Microsoft.OpenApi.Models;
using Nop.Core.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class AddSwaggerHeadersOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (Singleton<ApiHeadersOperations>.Instance.TryGetValue(context.ApiDescription.GroupName, out var value))
		{
			operation.Parameters = value.GetApiParameters();
		}
	}
}
