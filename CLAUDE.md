# Ecommerce Microservices

.NET 8 microservices + Angular 18 client. Mỗi service theo Clean Architecture 4 lớp:
`API` → `Application` → `Infrastructure` → `Core`. Giao tiếp bằng 3 kiểu: REST (qua Ocelot
gateway), gRPC (Basket → Discount), và message async (Basket → RabbitMQ → Ordering).

## Lệnh thường dùng

```bash
# Build toàn bộ solution (~8s)
dotnet build Ecommerce.sln

# Chạy test (~0.5s). PHẢI chỉ định .sln — thư mục gốc có cả Ecommerce.sln
# lẫn docker-compose.dcproj nên `dotnet test` trần sẽ báo lỗi MSB1011.
dotnet test Ecommerce.sln
dotnet test Tests/Catalog.Application.Tests/Catalog.Application.Tests.csproj   # chạy 1 project

# Dựng toàn bộ stack (13 container)
docker compose up -d --build
docker compose ps                 # kiểm tra trạng thái
docker compose logs -f <service>  # xem log 1 service

# EF migration cho Ordering (project có sẵn IDesignTimeDbContextFactory)
dotnet ef migrations add <Name> -p Services/Ordering/Ordering.Infrastructure -s Services/Ordering/Ordering.API
dotnet ef database update        -p Services/Ordering/Ordering.Infrastructure -s Services/Ordering/Ordering.API

# Frontend (node_modules CHƯA được cài)
cd client && npm install && npm start
```

⚠️ Trước khi `docker compose up`, chạy `docker ps` kiểm tra port 8000–8003 có bị stack khác
chiếm không.

## Bản đồ port

| Thành phần | Port (host) | Datastore |
|---|---|---|
| Ocelot API Gateway | 8010 | — |
| Catalog.API | 8000 | MongoDB 27017 |
| Basket.API | 8001 | Redis 6379 |
| Discount.API | 8002 | PostgreSQL 5432 |
| Ordering.API | 8003 | SQL Server 1433 |
| RabbitMQ | 5672 / 15672 (UI) | — |
| Elasticsearch / Kibana | 9200 / 5601 | — |
| pgAdmin / Portainer | 5050 / 9090 | — |

Trong container, các service gọi nhau bằng **service name** (`http://discount.api:8080`),
không phải localhost. Mọi service lắng nghe cổng 8080 bên trong container.

## Quy ước BẮT BUỘC

1. **Thêm/sửa endpoint ⇒ PHẢI cập nhật `ApiGateways/Ocelot.ApiGateway/ocelot.Development.json`.**
   Đây là nguồn lỗi số một của repo. Sau khi sửa, đối chiếu `DownstreamPathTemplate` khớp
   **chính xác** route thật của controller (`api/v{version}/[controller]/[action]`).

2. **KHÔNG thêm package Serilog vào csproj của service.** Chúng chảy transitively từ
   `Infrastructure/Common.Logging`. Muốn đổi cấu hình log → sửa `Common.Logging/Logging.cs`,
   áp dụng đồng thời cho cả 4 service. Kiểm tra bằng
   `dotnet list <proj> package --include-transitive`.

3. **Luồng CQRS**: Controller → MediatR → Handler → Repository. Controller **không** gọi
   thẳng repository.
   - Command/Query → `Services/<Svc>/<Svc>.Application/Commands|Queries/`
   - Handler → `<Svc>.Application/Handlers/`
   - Entity → `<Svc>.Core/Entities/`
   - Interface repository → `<Svc>.Core/Repositories/`, implement ở `<Svc>.Infrastructure/`

4. **Structured logging**: dùng `_logger.LogInformation("Text {Prop}", value)`.
   KHÔNG dùng `$"..."` — nội suy chuỗi làm mất field searchable trên Kibana.
   Code hiện tại đang sai chỗ này ở hầu hết controller; sửa dần khi đụng vào.

5. **gRPC contract dùng chung**: `Services/Discount/Discount.Application/Protos/discount.proto`.
   Basket link trực tiếp tới file này qua `<Protobuf Include="..." GrpcServices="Client">`.
   Sửa proto ⇒ ảnh hưởng cả hai service, build lại cả hai.

6. **Ordering** dùng MediatR Pipeline Behaviors (`ValidationBehavior`, `UnhandledExceptionBehavior`).
   Thêm command mới ⇒ thêm luôn FluentValidation validator trong `Ordering.Application/Validators/`,
   nếu không command sẽ đi qua mà không được validate.

7. **Event versioning**: `BasketCheckoutEvent` (v1) và `BasketCheckoutEventV2` dùng **hai queue
   riêng** khai báo ở `EventBus.Messages/Common/EventBusConstant.cs`, hai consumer riêng.
   Không sửa breaking vào event cũ — tạo version mới theo đúng nếp này.

## Đặc điểm cần lưu ý

- **Discount.API chỉ có gRPC, KHÔNG có REST controller.** Kestrel bị ép `Protocols: Http2`.
  Mọi route REST trỏ vào Discount đều không hoạt động.
- **Catalog seed dữ liệu trong constructor của `CatalogContext`** từ
  `Catalog.Infrastructure/Data/SeedData/*.json`.
- **Ordering migrate + seed lúc khởi động** qua `MigrateDatabase<OrderContext>()` với Polly retry.
## Test

`Tests/Catalog.Application.Tests` — unit test cho các handler của Catalog (xUnit + NSubstitute
+ Shouldly). Mock `IProductRepository`/`IBrandRepository`/`ITypeRepository`, không đụng MongoDB.

- Package và property dùng chung khai báo ở `Tests/Directory.Build.props`. Thêm test project
  mới chỉ cần tạo csproj với đúng một `ProjectReference` — mọi thứ khác tự thừa hưởng.
- Chưa có test cho Basket / Discount / Ordering.
- `ProductMapper.Mapper` là static nên không mock được — mapping chạy thật trong test,
  nghĩa là test đồng thời phủ luôn `ProductMappingProfile`.

⚠️ Máy dev này có NuGet source `LocalNuget` (`https://192.168.1.10:5000/nuget`) thường không
truy cập được và làm treo restore. Khi thêm package, chỉ định thẳng nguồn:
`dotnet add <proj> package <Name> --source https://api.nuget.org/v3/index.json`

## Nợ kỹ thuật đã biết (không cần báo cáo lại)

- **Không có authentication/authorization.** Basket/Order định danh bằng `userName` trên URL
  → bất kỳ ai cũng đọc/xóa được dữ liệu người khác (IDOR).
- **Lưu dữ liệu thẻ thanh toán plaintext** (`CardNumber`, `Cvv`) trong `Order` entity và
  truyền qua RabbitMQ.
- **Credential hardcode** trong `appsettings.json` và `docker-compose.override.yml`, đã commit git.
- **Discount migration `DROP TABLE IF EXISTS Coupon`** mỗi lần khởi động → mất sạch coupon.
- **Nhiều route Ocelot lệch tên với controller** (Catalog `GetProductsByBrandName` vs
  `GetProductByBrandName`; route `/api/v1/Catalog` không map action nào; DELETE product
  không có route).
- **Kibana dùng sai biến env** `ELASTICSEARCH_URL` — Kibana 8 cần `ELASTICSEARCH_HOSTS`.
- **`IndexFormat` có chữ hoa** (`ecommerce-Logs-`) nhưng ES lowercase tên index → data view
  Kibana phải dùng pattern `ecommerce-logs-*`.
- **Angular client chưa hoạt động**: `StoreModule` không được import ở đâu, `baseUrl` sai
  (`9010`, thiếu `/` cuối), `store.component.ts` trỏ tới `.css` trong khi file là `.scss`.
- **Health check package đã cài nhưng chưa wire** (`AddHealthChecks()` chưa được gọi).
- **Ocelot gateway không dùng Serilog** → log lớp gateway không vào ELK.
- **Ocelot dùng `host.docker.internal`** thay vì service name của Docker network.
- 84 compiler warning, chủ yếu CS8618/CS8602 (nullable reference chưa xử lý).
