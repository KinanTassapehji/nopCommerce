using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.Product360View.Domain;
using NopStation.Plugin.Widgets.Product360View.Models;

namespace NopStation.Plugin.Widgets.Product360View.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<ProductImageSetting360, ImageSetting360Model>().ReverseMap();
		CreateMap<ProductPictureMapping360, ProductPicture360Model>().ReverseMap();
		CreateMap<Product360ViewSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.IsEnabled_OverrideForStore, delegate(IMemberConfigurationExpression<Product360ViewSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<Product360ViewSettings, ConfigurationModel, int> options)
		{
			options.Ignore();
		});
		CreateMap<ConfigurationModel, Product360ViewSettings>();
	}
}
