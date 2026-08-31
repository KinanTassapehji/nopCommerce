using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.AdminReportExporter.Areas.Admin.Model;

namespace NopStation.Plugin.Misc.AdminReportExporter.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<AdminReportExporterSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.EnablePlugin_OverrideForStore, delegate(IMemberConfigurationExpression<AdminReportExporterSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<AdminReportExporterSettings, ConfigurationModel, int> options)
		{
			options.Ignore();
		});
		CreateMap<ConfigurationModel, AdminReportExporterSettings>();
	}
}
