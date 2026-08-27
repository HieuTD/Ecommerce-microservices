using Catalog.Application.Handlers;
using Catalog.Application.Queries;
using Catalog.Application.Tests.TestData;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;

namespace Catalog.Application.Tests.Handlers;

/// <summary>
/// Các handler truy vấn sản phẩm: theo Id, theo tên, theo brand.
/// </summary>
public class ProductQueryHandlersTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();

    [Fact]
    public async Task GetProductById_MapsEntityToResponse()
    {
        _repository.GetProduct(ProductFactory.AdidasId).Returns(ProductFactory.AdidasShoes());

        var result = await new GetProductByIdQueryHandler(_repository)
            .Handle(new GetProductByIdQuery(ProductFactory.AdidasId), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(ProductFactory.AdidasId);
        result.Name.ShouldBe("Adidas Quick Force Indoor Badminton Shoes");
        result.Price.ShouldBe(2500m);
        result.ImageFile.ShouldBe("product-1.png");
    }

    [Fact]
    public async Task GetProductById_QueriesRepositoryWithGivenId()
    {
        _repository.GetProduct(Arg.Any<string>()).Returns(ProductFactory.AdidasShoes());

        await new GetProductByIdQueryHandler(_repository)
            .Handle(new GetProductByIdQuery("some-id"), CancellationToken.None);

        await _repository.Received(1).GetProduct("some-id");
    }

    [Fact]
    public async Task GetProductByName_MapsAllMatches()
    {
        _repository.GetProductsByName("badminton")
            .Returns(new[] { ProductFactory.AdidasShoes(), ProductFactory.YonexRacquet() });

        var result = await new GetProductByNameQueryHandler(_repository)
            .Handle(new GetProductByNameQuery("badminton"), CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Select(p => p.Name).ShouldContain("Adidas Quick Force Indoor Badminton Shoes");
    }

    [Fact]
    public async Task GetProductByBrand_MapsAllMatches()
    {
        _repository.GetProductsByBrand("Adidas").Returns(new[] { ProductFactory.AdidasShoes() });

        var result = await new GetProductByBrandQueryHandler(_repository)
            .Handle(new GetProductByBrandQuery("Adidas"), CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Brands.Name.ShouldBe("Adidas");
    }

    [Fact]
    public async Task GetProductByBrand_WithNoMatches_ReturnsEmptyList()
    {
        _repository.GetProductsByBrand(Arg.Any<string>()).Returns(Array.Empty<Product>());

        var result = await new GetProductByBrandQueryHandler(_repository)
            .Handle(new GetProductByBrandQuery("KhongTonTai"), CancellationToken.None);

        result.ShouldBeEmpty();
    }
}
