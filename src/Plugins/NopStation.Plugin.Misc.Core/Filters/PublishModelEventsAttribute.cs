using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.Core.Filters;

public sealed class PublishModelEventsAttribute : TypeFilterAttribute
{
	private class PublishModelEventsFilter : IAsyncActionFilter, IFilterMetadata, IAsyncResultFilter
	{
		private readonly bool _ignoreFilter;

		private readonly IEventPublisher _eventPublisher;

		public PublishModelEventsFilter(bool ignoreFilter, IEventPublisher eventPublisher)
		{
			_ignoreFilter = ignoreFilter;
			_eventPublisher = eventPublisher;
		}

		protected virtual bool IgnoreFilter(FilterContext context)
		{
			return (from filterDescriptor in context.ActionDescriptor.FilterDescriptors
				where filterDescriptor.Scope == FilterScope.Action
				select filterDescriptor.Filter).OfType<PublishModelEventsAttribute>().FirstOrDefault()?.IgnoreFilter ?? _ignoreFilter;
		}

		protected virtual async Task PublishModelPreparedEventAsync(object model)
		{
			if (model != null && model.GetType().IsGenericType && !(model.GetType().GetGenericTypeDefinition() != typeof(GenericResponseModel<>)))
			{
				object responseModel = model.GetType().GetProperty("Data").GetValue(model);
				if (responseModel is BaseNopModel model2)
				{
					await _eventPublisher.ModelPreparedAsync(model2);
				}
				if (responseModel is IEnumerable<BaseNopModel> model3)
				{
					await _eventPublisher.ModelPreparedAsync(model3);
				}
			}
		}

		private async Task PublishModelReceivedEventAsync(ActionExecutingContext context)
		{
			ArgumentNullException.ThrowIfNull(context, "context");
			if (!context.HttpContext.Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) || IgnoreFilter(context))
			{
				return;
			}
			foreach (object value in context.ActionArguments.Values)
			{
				if (value != null && value.GetType().IsGenericType && !(value.GetType().GetGenericTypeDefinition() != typeof(BaseQueryModel<>)) && value.GetType().GetProperty("Data").GetValue(value) is BaseNopModel model)
				{
					await _eventPublisher.ModelReceivedAsync(model, context.ModelState);
				}
			}
		}

		private async Task PublishModelPreparedEventAsync(ActionExecutingContext context)
		{
			ArgumentNullException.ThrowIfNull(context, "context");
			if (!IgnoreFilter(context) && context.Controller is NopStationApiController nopStationApiController)
			{
				await PublishModelPreparedEventAsync(nopStationApiController.ViewData.Model);
			}
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			await PublishModelReceivedEventAsync(context);
			if (context.Result == null)
			{
				await next();
			}
			await PublishModelPreparedEventAsync(context);
		}

		public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			ArgumentNullException.ThrowIfNull(context, "context");
			if (!IgnoreFilter(context))
			{
				if (context.Result is ObjectResult objectResult)
				{
					await PublishModelPreparedEventAsync(objectResult.Value);
				}
				await next();
			}
		}
	}

	public bool IgnoreFilter { get; }

	public PublishModelEventsAttribute(bool ignore = false)
		: base(typeof(PublishModelEventsFilter))
	{
		IgnoreFilter = ignore;
		base.Arguments = new object[1] { ignore };
	}
}
