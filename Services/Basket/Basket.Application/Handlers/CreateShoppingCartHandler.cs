using Basket.Application.Commands;
using Basket.Application.Mappers;
using Basket.Application.Responses;
using Basket.Core.Entities;
using Basket.Core.Repositories;
using MediatR;

namespace Basket.Application.Handlers
{
    public class CreateShoppingCartHandler : IRequestHandler<CreateShoppingCartCommand, ShoppingCartResponse>
    {
        private readonly IBasketRepository _basketRepository;

        public CreateShoppingCartHandler(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
        }
        public async Task<ShoppingCartResponse> Handle(CreateShoppingCartCommand request, CancellationToken cancellationToken)
        {
            var basket = await _basketRepository.CreateOrUpdateBasket(new ShoppingCart
            {
                UserName = request.UserName,
                Items = request.Items
            });
            var basketResponse = BasketMapper.Mapper.Map<ShoppingCartResponse>(basket);
            return basketResponse;
        }
    }
}
