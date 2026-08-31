namespace NopStation.Plugin.Misc.Core.Services;

public interface IApiDescriptor
{
	string ApiGroup { get; }

	string ApiTitle { get; }

	string ApiVersion { get; }

	string ApiDescription { get; }
}
