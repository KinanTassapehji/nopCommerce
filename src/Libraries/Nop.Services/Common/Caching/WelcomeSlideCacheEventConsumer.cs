using Nop.Core.Domain.Common;
using Nop.Services.Caching;

namespace Nop.Services.Common.Caching;

/// <summary>
/// Represents a welcome slide cache event consumer
/// </summary>
public partial class WelcomeSlideCacheEventConsumer : CacheEventConsumer<WelcomeSlide>;