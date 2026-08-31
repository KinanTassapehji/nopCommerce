using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class SwaggerStartup : INopStartup
{
	public int Order => 0;

	public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(delegate(SwaggerGenOptions c)
		{
			ITypeFinder instance = Singleton<ITypeFinder>.Instance;
			IEnumerable<IApiDescriptor> apiDescriptors = from desc in instance.FindClassesOfType<IApiDescriptor>()
				select (IApiDescriptor)Activator.CreateInstance(desc) into desc
				where desc != null
				select desc;
			foreach (IApiDescriptor item in apiDescriptors)
			{
				c.SwaggerDoc(item.ApiGroup, new OpenApiInfo
				{
					Title = item.ApiTitle,
					Version = item.ApiVersion,
					Description = item.ApiDescription
				});
			}
			c.OperationFilter<AddSwaggerHeadersOperationFilter>(Array.Empty<object>());
			c.DocInclusionPredicate(delegate(string docName, ApiDescription apiDescription)
			{
				foreach (IApiDescriptor item2 in apiDescriptors)
				{
					if (docName == item2.ApiGroup && apiDescription.GroupName == item2.ApiGroup)
					{
						return true;
					}
				}
				return false;
			});
			c.ResolveConflictingActions((IEnumerable<ApiDescription> apiDescriptions) => apiDescriptions.First());
		});
	}

	public void Configure(IApplicationBuilder application)
	{
		ITypeFinder typeFinder = Singleton<ITypeFinder>.Instance;
		application.UseSwagger();
		application.UseSwaggerUI(delegate(SwaggerUIOptions c)
		{
			foreach (IApiDescriptor item in from desc in typeFinder.FindClassesOfType<IApiDescriptor>()
				select (IApiDescriptor)Activator.CreateInstance(desc) into desc
				where desc != null
				select desc)
			{
				c.SwaggerEndpoint("/swagger/" + item.ApiGroup + "/swagger.json", item.ApiTitle);
				c.RoutePrefix = "swagger/" + item.ApiGroup;
			}
		});
		IEnumerable<IHeadersOperation> enumerable = from op in typeFinder.FindClassesOfType<IHeadersOperation>()
			select (IHeadersOperation)Activator.CreateInstance(op) into op
			where op != null
			select op;
		ApiHeadersOperations apiHeadersOperations = new ApiHeadersOperations();
		foreach (IHeadersOperation item2 in enumerable)
		{
			apiHeadersOperations[item2.GroupName] = item2;
		}
		Singleton<ApiHeadersOperations>.Instance = apiHeadersOperations;
	}
}
