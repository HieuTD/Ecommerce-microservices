using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Core.Specs;
using Catalog.Infrastructure.Data;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository, IBrandRepository, ITypeRepository
    {
        public ICatalogContext _context { get; }
        public ProductRepository(ICatalogContext context)
        {
            _context = context;
        }

        public async Task<Product> GetProduct(string id)
        {
            return await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Pagination<Product>> GetProducts(CatalogSpecParams catalogSpecParams)
        {
            var builder = Builders<Product>.Filter;
            var filtered = builder.Empty;

            if (!String.IsNullOrEmpty(catalogSpecParams.Search))
            {
                filtered = filtered & builder.Where(p => p.Name.ToLower().Contains(catalogSpecParams.Search));
            }
            if (!string.IsNullOrEmpty(catalogSpecParams.BrandId))
            {
                var brandFiltered = builder.Eq(p => p.Brands.Id, catalogSpecParams.BrandId);
                filtered = filtered & brandFiltered;
            }
            if (!string.IsNullOrEmpty(catalogSpecParams.TypeId))
            {
                var typeFiltered = builder.Eq(p => p.Types.Id, catalogSpecParams.TypeId);
                filtered = filtered & typeFiltered;
            }

            var totalItems = await _context.Products.CountDocumentsAsync(filtered);
            var data = await DataFilter(catalogSpecParams, filtered);

            return new Pagination<Product>(
                    catalogSpecParams.PageIndex,
                    catalogSpecParams.PageSize,
                    (int)totalItems,
                    data
                );
        }

        public async Task<IEnumerable<Product>> GetProductsByBrand(string brandName)
        {
            return await _context.Products.Find(p => p.Brands.Name.ToLower() == brandName.ToLower()).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByName(string name)
        {
            return await _context.Products.Find(p => p.Name.ToLower().Contains(name.ToLower())).ToListAsync();
        }

        public async Task<Product> CreateProduct(Product product)
        {
            await _context.Products.InsertOneAsync(product);
            return product;
        }

        public async Task<bool> DeleteProduct(string id)
        {
            var deletedProduct = await _context.Products.DeleteOneAsync(p => p.Id == id);
            return deletedProduct.IsAcknowledged && deletedProduct.DeletedCount > 0;
        }

        public async Task<bool> UpdateProduct(Product product)
        {
            var updatedProduct = await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
            return updatedProduct.IsAcknowledged && updatedProduct.ModifiedCount > 0;
        }

        public async Task<IEnumerable<ProductBrand>> GetAllBrands()
        {
            return await _context.Brands.Find(b => true).ToListAsync();
        }

        public async Task<IEnumerable<ProductType>> GetAllTypes()
        {
            return await _context.Types.Find(t => true).ToListAsync();
        }

        private async Task<IReadOnlyList<Product>> DataFilter(CatalogSpecParams catalogSpecParams, FilterDefinition<Product> filtered)
        {
            var sortDefn = Builders<Product>.Sort.Ascending("Name");
            if (!string.IsNullOrEmpty(catalogSpecParams.Sort))
            {
                switch (catalogSpecParams.Sort)
                {
                    case "priceAsc":
                        sortDefn = Builders<Product>.Sort.Ascending(p => p.Price);
                        break;
                    case "priceDesc":
                        sortDefn = Builders<Product>.Sort.Descending(p => p.Price);
                        break;
                    default:
                        Builders<Product>.Sort.Ascending(p => p.Name);
                        break;
                }
            }
            return await _context.Products
                .Find(filtered)
                .Sort(sortDefn)
                .Skip((catalogSpecParams.PageIndex - 1) * catalogSpecParams.PageSize)
                .Limit(catalogSpecParams.PageSize)
                .ToListAsync();
        }
    }
}
