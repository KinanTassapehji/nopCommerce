using Nop.Core;
using Nop.Core.Caching;

namespace NopStation.Plugin.Misc.Core.Caching;

public static class NopStationEntityCacheDefaults<TEntity> where TEntity : BaseEntity
{
	public static string EntityTypeName => typeof(TEntity).Namespace.ToLowerInvariant();

	public static CacheKey ByIdCacheKey => new CacheKey(EntityTypeName + ".byid.{0}");

	public static CacheKey ByIdsCacheKey => new CacheKey(EntityTypeName + ".byids.{0}");

	public static CacheKey AllCacheKey => new CacheKey(EntityTypeName + ".all.");

	public static string Prefix => EntityTypeName + ".";

	public static string ByIdPrefix => EntityTypeName + ".byid.";

	public static string ByIdsPrefix => EntityTypeName + ".byids.";

	public static string AllPrefix => EntityTypeName + ".all.";
}
