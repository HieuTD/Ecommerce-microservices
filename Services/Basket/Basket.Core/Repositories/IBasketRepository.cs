using Basket.Core.Entities;

namespace Basket.Core.Repositories
{
    public interface IBasketRepository
    {
        Task<ShoppingCart> GetBasket(string userName);
        Task<ShoppingCart> CreateOrUpdateBasket(ShoppingCart shoppingCart);
        Task DeleteBasket(string userName);
    }
}
