using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Data;

namespace NopStation.Plugin.Misc.Core.Services;

public abstract class BaseEntityService<TEntity> where TEntity : BaseEntity
{
	protected readonly IRepository<TEntity> _repository;

	protected readonly IStaticCacheManager _cacheManager;

	protected readonly IEventPublisher _eventPublisher;

	protected BaseEntityService(IRepository<TEntity> repository, IStaticCacheManager cacheManager, IEventPublisher eventPublisher)
	{
		_repository = repository;
		_cacheManager = cacheManager;
		_eventPublisher = eventPublisher;
	}

	public virtual async Task<TEntity> GetByIdAsync(int id, bool includeDeleted = false)
	{
		if (id == 0)
		{
			return null;
		}
		return await _repository.GetByIdAsync(id, (ICacheKeyService cache) => (CacheKey)null);
	}

	public virtual async Task<IPagedList<TEntity>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
	{
		return await _repository.Table.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
	}

	public virtual async Task InsertAsync(TEntity entity)
	{
		await _repository.InsertAsync(entity);
	}

	public virtual async Task UpdateAsync(TEntity entity)
	{
		await _repository.UpdateAsync(entity);
	}

	public virtual async Task DeleteAsync(TEntity entity)
	{
		await _repository.DeleteAsync(entity);
	}

	public virtual async Task InsertAsync(IList<TEntity> entities)
	{
		await _repository.InsertAsync(entities);
	}

	public virtual async Task DeleteAsync(IList<TEntity> entities)
	{
		await _repository.DeleteAsync(entities);
	}
}
