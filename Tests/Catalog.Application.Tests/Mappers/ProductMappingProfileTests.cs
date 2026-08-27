using Catalog.Application.Mappers;

namespace Catalog.Application.Tests.Mappers;

/// <summary>
/// Kiểm tra cấu hình AutoMapper. Test này bắt được lỗi thiếu CreateMap —
/// loại lỗi mà compiler không thấy nhưng làm sập endpoint lúc runtime.
/// </summary>
public class ProductMappingProfileTests
{
    [Fact]
    public void MappingConfiguration_IsValid()
    {
        // Ném AutoMapperConfigurationException nếu có destination member nào
        // không tìm được nguồn tương ứng.
        Should.NotThrow(() => ProductMapper.Mapper.ConfigurationProvider.AssertConfigurationIsValid());
    }
}
