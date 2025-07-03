using Basket.Application.Commands;
using Basket.Core.Repositories;
using MediatR;

namespace Basket.Application.Handlers
{
    public class DeleteShoppingCartHandler : IRequestHandler<DeleteShoppingCartCommand, Unit>
    {
        private readonly IBasketRepository _basketRepository;

        public DeleteShoppingCartHandler(IBasketRepository basketRepository)
        {
            _basketRepository = basketRepository;
        }

        public async Task<Unit> Handle(DeleteShoppingCartCommand request, CancellationToken cancellationToken)
        {
            await _basketRepository.DeleteBasket(request.UserName);
            return Unit.Value;
        }
    }
}
