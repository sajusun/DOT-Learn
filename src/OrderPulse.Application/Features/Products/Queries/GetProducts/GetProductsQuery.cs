using MediatR;
using OrderPulse.Application.Common.DTOs;
using OrderPulse.Application.Common.Interfaces;
using OrderPulse.Domain.Repositories;

namespace OrderPulse.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;

    public GetProductsQueryHandler(IProductRepository productRepository, ICacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // Cache-Aside Pattern with Redis
        // In Laravel: Cache::remember('products:active', now()->addMinutes(5), fn() => ...)
        const string cacheKey = "products:active";

        var products = await _cacheService.GetOrCreateAsync<IReadOnlyList<ProductDto>>(
            cacheKey,
            async () =>
            {
                var entities = await _productRepository.GetAllActiveAsync(cancellationToken);
                return entities.Select(p => new ProductDto(
                    p.Id,
                    p.Sku,
                    p.Name,
                    p.Description,
                    p.Price.Amount,
                    p.Price.Currency,
                    p.AvailableStock,
                    p.IsActive
                )).ToList();
            },
            expiration: TimeSpan.FromMinutes(5),
            cancellationToken: cancellationToken
        );

        return products ?? [];
    }
}
