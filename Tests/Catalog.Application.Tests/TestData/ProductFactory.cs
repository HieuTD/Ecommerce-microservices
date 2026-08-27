using Catalog.Core.Entities;

namespace Catalog.Application.Tests.TestData;

/// <summary>
/// Tạo dữ liệu mẫu cho test. Gom về một chỗ để khi entity đổi shape
/// chỉ phải sửa ở đây thay vì sửa rải rác trong từng test.
/// </summary>
internal static class ProductFactory
{
    internal const string AdidasId = "60ee1d4b1f2a3c0001b2c3d4";
    internal const string YonexId = "60ee1d4b1f2a3c0001b2c3d5";

    internal static ProductBrand Adidas() => new() { Id = "brand-adidas", Name = "Adidas" };

    internal static ProductType Shoes() => new() { Id = "type-shoes", Name = "Shoes" };

    internal static Product AdidasShoes() => new()
    {
        Id = AdidasId,
        Name = "Adidas Quick Force Indoor Badminton Shoes",
        Summary = "Badminton shoes",
        Description = "Lightweight indoor badminton shoes",
        ImageFile = "product-1.png",
        Brands = Adidas(),
        Types = Shoes(),
        Price = 2500m
    };

    internal static Product YonexRacquet() => new()
    {
        Id = YonexId,
        Name = "Yonex VCORE Pro 100 A Tennis Racquet",
        Summary = "Tennis racquet",
        Description = "270gm, strung",
        ImageFile = "product-2.png",
        Brands = new ProductBrand { Id = "brand-yonex", Name = "Yonex" },
        Types = new ProductType { Id = "type-racquet", Name = "Racquet" },
        Price = 7000m
    };
}
