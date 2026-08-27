using Catalog.Application.Handlers;
using Catalog.Application.Queries;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;

namespace Catalog.Application.Tests.Handlers;

public class BrandAndTypeHandlersTests
{
    [Fact]
    public async Task GetAllBrands_MapsEveryBrandToResponse()
    {
        var repository = Substitute.For<IBrandRepository>();
        repository.GetAllBrands().Returns(new[]
        {
            new ProductBrand { Id = "brand-adidas", Name = "Adidas" },
            new ProductBrand { Id = "brand-yonex",  Name = "Yonex"  }
        });

        var result = await new GetAllBrandsHandler(repository)
            .Handle(new GetAllBrandsQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe("brand-adidas");
        result[0].Name.ShouldBe("Adidas");
        result[1].Name.ShouldBe("Yonex");
    }

    [Fact]
    public async Task GetAllTypes_MapsEveryTypeToResponse()
    {
        var repository = Substitute.For<ITypeRepository>();
        repository.GetAllTypes().Returns(new[]
        {
            new ProductType { Id = "type-shoes",   Name = "Shoes"   },
            new ProductType { Id = "type-racquet", Name = "Racquet" }
        });

        var result = await new GetAllTypesHandler(repository)
            .Handle(new GetAllTypesQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe("type-shoes");
        result[1].Name.ShouldBe("Racquet");
    }

    [Fact]
    public async Task GetAllBrands_WithEmptyCollection_ReturnsEmptyList()
    {
        var repository = Substitute.For<IBrandRepository>();
        repository.GetAllBrands().Returns(Array.Empty<ProductBrand>());

        var result = await new GetAllBrandsHandler(repository)
            .Handle(new GetAllBrandsQuery(), CancellationToken.None);

        result.ShouldBeEmpty();
    }
}
