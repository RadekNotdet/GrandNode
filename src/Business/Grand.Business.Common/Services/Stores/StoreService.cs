#nullable enable

using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Data;
using Grand.Domain.Stores;
using Grand.Infrastructure.Caching;
using Grand.Infrastructure.Caching.Constants;
using Grand.Infrastructure.Extensions;
using MediatR;

namespace Grand.Business.Common.Services.Stores;

/// <summary>
///     Store service
/// </summary>
public class StoreService : IStoreService
{
    #region Ctor

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="cacheBase">Cache manager</param>
    /// <param name="storeRepository">Store repository</param>
    /// <param name="mediator">Mediator</param>
    public StoreService(ICacheBase cacheBase,
        IRepository<Store> storeRepository,
        IMediator mediator)
    {
        _cacheBase = cacheBase;
        _storeRepository = storeRepository;
        _mediator = mediator;
    }

    #endregion

    #region Fields

    private readonly IRepository<Store> _storeRepository;
    private readonly IMediator _mediator;
    private readonly ICacheBase _cacheBase;

    #endregion

    #region Methods

    /// <summary>
    ///     Gets all stores
    /// </summary>
    /// <returns>Stores</returns>
    public virtual async Task<IList<Store>> GetAllStores()
    {
        return await _cacheBase.GetAsync(CacheKey.STORES_ALL_KEY, async () =>
        {
            return await Task.FromResult(_storeRepository.Table.OrderBy(x => x.DisplayOrder).ToList());
        });
    }

    /// <summary>
    ///     Gets a store
    /// </summary>
    /// <param name="storeId">Store identifier</param>
    /// <returns>Store</returns>
    public virtual Task<Store> GetStoreById(string storeId)
    {
        var key = string.Format(CacheKey.STORES_BY_ID_KEY, storeId);
        return _cacheBase.GetAsync(key, () => _storeRepository.GetByIdAsync(storeId));
    }

    /// <summary>
    ///     Inserts a store
    /// </summary>
    /// <param name="store">Store</param>
    public virtual async Task InsertStore(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        await _storeRepository.InsertAsync(store);

        //clear cache
        await _cacheBase.Clear();

        //event notification
        await _mediator.EntityInserted(store);
    }

    /// <summary>
    ///     Updates the store
    /// </summary>
    /// <param name="store">Store</param>
    public virtual async Task UpdateStore(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        await _storeRepository.UpdateAsync(store);

        //clear cache
        await _cacheBase.Clear();

        //event notification
        await _mediator.EntityUpdated(store);
    }

    /// <summary>
    ///     Deletes a store
    /// </summary>
    /// <param name="store">Store</param>
    public virtual async Task DeleteStore(Store store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var allStores = await GetAllStores();
        if (allStores.Count == 1)
            throw new Exception("You cannot delete the only configured store");

        await _storeRepository.DeleteAsync(store);

        //clear cache
        await _cacheBase.Clear();

        //event notification
        await _mediator.EntityDeleted(store);
    }
    /// <summary>
    /// Get store by host
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public async Task<Store?> GetStoreByHost(string host)
    {
        var allStores = await GetAllStores();
        return allStores.FirstOrDefault(s => s.ContainsHostValue(host));
    }

    #endregion
}