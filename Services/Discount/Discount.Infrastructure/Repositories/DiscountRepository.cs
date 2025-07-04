using Dapper;
using Discount.Core.Entities;
using Discount.Core.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Discount.Infrastructure.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;

        public DiscountRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<Coupon> GetDiscount(string productName)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>
                ("SELECT * FROM Coupon WHERE ProductName = @ProductName", new { ProductName = productName });
            if (coupon == null)
            {
                return new Coupon() { ProductName = "No Discount", Amount = 0, Description = "No Discount Available" };
            }
            return coupon;
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var result = await connection.ExecuteAsync
                ("INSERT INTO Coupon(ProductName, Amount, Description) VALUES (@ProductName, @Amount, @Description)",
                new { ProductName = coupon.ProductName, Amount = coupon.Amount, Description = coupon.Description });

            if (result == 0)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var result = await connection.ExecuteAsync
                ("UPDATE Coupon set ProductName = @ProductName, Amount = @Amount, Description = @Description WHERE Id = @Id",
                new { ProductName = coupon.ProductName, Amount = coupon.Amount, Description = coupon.Description, Id = coupon.Id });

            if (result == 0)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            await using var connection = new NpgsqlConnection(_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var result = await connection.ExecuteAsync
                ("DELETE FROM Coupon WHERE ProductName = @ProductName", new { ProductName = productName });

            if (result == 0)
            {
                return false;
            }
            return true;
        }
    }
}
