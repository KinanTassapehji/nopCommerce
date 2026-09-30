using Nop.Core.Caching;
using Nop.Core.Domain.Security;
using Nop.Services.Caching;

namespace Nop.Services.Security.Caching;

/// <summary>
/// Represents a permission record-customer role mapping cache event consumer
/// </summary>
public partial class PermissionRecordCustomerRoleMappingCacheEventConsumer : CacheEventConsumer<PermissionRecordCustomerRoleMapping>
{
    /// <summary>
    /// Clear cache data
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task ClearCacheAsync(PermissionRecordCustomerRoleMapping entity)
    {
        //the mapping knows only the permission id, and grants change rarely: drop every role's permission list
        //("Nop.permissionrecord.all.{roleId}") and every "allowed" answer
        await RemoveByPrefixAsync(NopEntityCacheDefaults<PermissionRecord>.AllPrefix);
        await RemoveByPrefixAsync(NopSecurityDefaults.PermissionAllowedPrefix, string.Empty);
    }
}