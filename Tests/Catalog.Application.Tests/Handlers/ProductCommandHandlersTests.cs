using Catalog.Application.Commands;
using Catalog.Application.Handlers;
using Catalog.Application.Tests.TestData;
using Catalog.Core.Entities;
using Catalog.Core.Repositories;

namespace Catalog.Application.Tests.Handlers;

public class ProductCommandHandlersTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();

    [Fact]
    public async Task CreateProduct_MapsCommandToEntityBeforeSaving()
    {
        _repository.CreateProduct(Arg.Any<Product>())
            .Returns(callInfo => callInfo.Arg<Product>());

        var command = new CreateProductCommand
        {
            Name = "Adidas Quick Force Indoor Badminton Shoes",
            Summary = "Badminton shoes",
            Description = "Lightweight indoor badminton shoes",
            ImageFile = "product-1.png",
            Brands = ProductFactory.Adidas(),
            Types = ProductFactory.Shoes(),
            Price = 2500m
        };

        var result = await new CreateProductCommandHandler(_repository)
            .Handle(command, CancellationToken.None);

        // Entity đưa xuống repository phải mang đúng dữ liệu của command
        await _repository.Received(1).CreateProduct(Arg.Is<Product>(p =>
            p.Name == "Adidas Quick Force Indoor Badminton Shoes" &&
            p.Price == 2500m &&
            p.ImageFile == "product-1.png"));

        // Và response trả về phải được map ngược từ entity vừa tạo
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Adidas Quick Force Indoor Badminton Shoes");
        result.Price.ShouldBe(2500m);
        result.Brands.Name.ShouldBe("Adidas");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteProduct_ReturnsRepositoryResultVerbatim(bool repositoryResult)
    {
        _repository.DeleteProduct(ProductFactory.AdidasId).Returns(repositoryResult);

        var result = await new DeleteProductByIdCommandHandler(_repository)
            .Handle(new DeleteProductByIdCommand(ProductFactory.AdidasId), CancellationToken.None);

        result.ShouldBe(repositoryResult);
        await _repository.Received(1).DeleteProduct(ProductFactory.AdidasId);
    }

    [Fact]
    public async Task UpdateProduct_MapsCommandToEntityAndReturnsRepositoryResult()
    {
        _repository.UpdateProduct(Arg.Any<Product>()).Returns(true);

        var command = new UpdateProductCommand
        {
            Id = ProductFactory.AdidasId,
            Name = "Adidas Quick Force Indoor Badminton Shoes v2",
            Summary = "Badminton shoes",
            Description = "Updated description",
            ImageFile = "product-1-v2.png",
            Brands = ProductFactory.Adidas(),
            Types = ProductFactory.Shoes(),
            Price = 2700m
        };

        var result = await new UpdateProductCommandHandler(_repository)
            .Handle(command, CancellationToken.None);

        result.ShouldBeTrue();
        await _repository.Received(1).UpdateProduct(Arg.Is<Product>(p =>
            p.Id == ProductFactory.AdidasId &&
            p.Name == "Adidas Quick Force Indoor Badminton Shoes v2" &&
            p.Price == 2700m));
    }
}
