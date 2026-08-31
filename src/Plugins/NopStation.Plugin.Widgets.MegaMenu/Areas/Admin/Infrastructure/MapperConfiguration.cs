using System.Collections.Generic;
using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;
using NopStation.Plugin.Widgets.MegaMenu.Domains;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
	public int Order => 1;

	public MapperConfiguration()
	{
		CreateMap<MegaMenuSettings, ConfigurationModel>().ForMember((ConfigurationModel model) => model.HideManufacturers_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.EnableMegaMenu_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		}).ForMember((ConfigurationModel model) => model.MaxCategoryLevelsToShow_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
		{
			options.Ignore();
		})
			.ForMember((ConfigurationModel model) => model.SelectedCategoryIds_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowCategoryPicture_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.SelectedManufacturerIds_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowNumberOfCategoryProductsIncludeSubcategories_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowNumberofCategoryProducts_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowManufacturerPicture_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowSubcategoryPicture_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ShowMainCategoryPictureRight_OverrideForStore, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, bool> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.SelectedManufacturerIds, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, IList<int>> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.SelectedCategoryIds, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, IList<int>> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.CustomProperties, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, Dictionary<string, string>> options)
			{
				options.Ignore();
			})
			.ForMember((ConfigurationModel model) => model.ActiveStoreScopeConfiguration, delegate(IMemberConfigurationExpression<MegaMenuSettings, ConfigurationModel, int> options)
			{
				options.Ignore();
			});
		CreateMap<ConfigurationModel, MegaMenuSettings>().ForMember((MegaMenuSettings entity) => entity.SelectedManufacturerIds, delegate(IMemberConfigurationExpression<ConfigurationModel, MegaMenuSettings, string> options)
		{
			options.Ignore();
		}).ForMember((MegaMenuSettings entity) => entity.SelectedCategoryIds, delegate(IMemberConfigurationExpression<ConfigurationModel, MegaMenuSettings, string> options)
		{
			options.Ignore();
		});
		CreateMap<CategoryIcon, CategoryIconModel>().ForMember((CategoryIconModel model) => model.CategoryName, delegate(IMemberConfigurationExpression<CategoryIcon, CategoryIconModel, string> options)
		{
			options.Ignore();
		}).ForMember((CategoryIconModel model) => model.PictureUrl, delegate(IMemberConfigurationExpression<CategoryIcon, CategoryIconModel, string> options)
		{
			options.Ignore();
		});
		CreateMap<CategoryIconModel, CategoryIcon>();
	}
}
