using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.Product360View.Infrastructure;

public class Picture360CacheKeys
{
	public static string Picture360Prefix = "Nop.product360viewpictures.{0}";

	public static CacheKey ImageSettingCacheKey = new CacheKey("Nop.product360view.imagesettings-{0}");

	public static string PictureSetting360Prefix = "Nop.product360viewsettings.{0}";

	public static CacheKey PictureMappingCacheKey = new CacheKey("Nop.product360view.picturemappings-{0}");

	public static string PictureMapping360Prefix = "Nop.product360viewmappings.{0}";

	public static CacheKey PicturesCacheKey => new CacheKey("Nop.product360view.pictures.-{0}-{1}");
}
