using Catalog.Application.Handlers;
using Catalog.Application.Queries;
using Catalog.Application.Tests.TestData;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Core.Specs;

namespace Catalog.Application.Tests.Handlers;

public class GetAllProductsHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();

    private GetAllProductsHandler CreateHandler() => new(_repository);

    [Fact]
    public async Task Handle_MapsPaginationMetadataAndItems()
    {
        var page = new Pagination<Product>(
            pageIndex: 2,
            pageSize: 10,
            count: 57,
            data: new[] { ProductFactory.AdidasShoes(), ProductFactory.YonexRacquet() });
        _repository.GetProducts(Arg.Any<CatalogSpecParams>()).Returns(page);

        var result = await CreateHandler().Handle(
            new GetAllProductsQuery(new CatalogSpecParams()), CancellationToken.None);

        // Metadata phân trang phải được giữ nguyên qua tầng mapping
        result.PageIndex.ShouldBe(2);
        result.PageSize.ShouldBe(10);
        result.Count.ShouldBe(57);

        // Items phải được map Product -> ProductResponse, kể cả object lồng nhau
        result.Data.Count.ShouldBe(2);
        result.Data[0].Name.ShouldBe("Adidas Quick Force Indoor Badminton Shoes");
        result.Data[0].Price.ShouldBe(2500m);
        result.Data[0].Brands.Name.ShouldBe("Adidas");
        result.Data[0].Types.Name.ShouldBe("Shoes");
        result.Data[1].Name.ShouldBe("Yonex VCORE Pro 100 A Tennis Racquet");
    }

    [Fact]
    public async Task Handle_PassesSpecParamsThroughToRepositoryUnchanged()
    {
        var specParams = new CatalogSpecParams
        {
            PageIndex = 3,
            PageSize = 20,
            BrandId = "brand-adidas",
            TypeId = "type-shoes",
            Search = "badminton",
            Sort = "priceAsc"
        };
        _repository.GetProducts(Arg.Any<CatalogSpecParams>())
            .Returns(new Pagination<Product>(3, 20, 0, Array.Empty<Product>()));

        await CreateHandler().Handle(new GetAllProductsQuery(specParams), CancellationToken.None);

        // Handler không được tự ý sửa đổi filter trước khi đưa xuống repository
        await _repository.Received(1).GetProducts(Arg.Is<CatalogSpecParams>(p =>
            p.PageIndex == 3 &&
            p.PageSize == 20 &&
            p.BrandId == "brand-adidas" &&
            p.TypeId == "type-shoes" &&
            p.Search == "badminton"));
    }

    [Fact]
    public async Task Handle_WithNoMatchingProducts_ReturnsEmptyDataNotNull()
    {
        _repository.GetProducts(Arg.Any<CatalogSpecParams>())
            .Returns(new Pagination<Product>(1, 10, 0, Array.Empty<Product>()));

        var result = await CreateHandler().Handle(
            new GetAllProductsQuery(new CatalogSpecParams()), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
        result.Data.ShouldBeEmpty();
    }
}
