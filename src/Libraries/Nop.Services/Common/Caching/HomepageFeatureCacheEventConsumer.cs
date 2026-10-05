using Nop.Core.Domain.Common;
using Nop.Services.Caching;

namespace Nop.Services.Common.Caching;

/// <summary>
/// Represents a home page feature cache event consumer
/// </summary>
public partial class HomepageFeatureCacheEventConsumer : CacheEventConsumer<HomepageFeature>;
