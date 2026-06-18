using Domain.DeliveryOptions;

namespace Infrastructure.Data.DeliveryOption;

internal sealed class DeliveryOptionRepository(AppDbContext dbContext)
    : RepositoryBase<DomainDeliveryOption>(dbContext),
        IDeliveryOptionRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public void CreateDeliveryOption(DomainDeliveryOption deliveryOption) => Create(deliveryOption);

    public void DeleteDeliveryOption(DomainDeliveryOption deliveryOption) => Delete(deliveryOption);

    public async Task<IReadOnlyList<DomainDeliveryOption>> GetDeliveryOptionsAsync(
        bool trackChanges,
        CancellationToken ct = default
    ) =>
        await (
            trackChanges
                ? _dbContext.DeliveryOptions.ToListAsync(ct)
                : _dbContext.DeliveryOptions.AsNoTracking().ToListAsync(ct)
        );

    public async Task<DomainDeliveryOption?> GetDeliveryOptionByIdAsync(
        DeliveryOptionId id,
        bool trackChanges,
        CancellationToken ct = default
    ) => await FindByCondition(x => x.Id == id, trackChanges).FirstOrDefaultAsync(ct);

    public async Task<DomainDeliveryOption?> GetDeliveryOptionByTitleAsync(
        DeliveryOptionTitle title,
        bool trackChanges,
        CancellationToken ct = default
    ) =>
        await FindByCondition(
                x => ((string)x.Title).ToLower() == title.Value.ToLower(),
                trackChanges
            )
            .FirstOrDefaultAsync(ct);

    public void UpdateDeliveryOption(DomainDeliveryOption deliveryOption) => Update(deliveryOption);
}
