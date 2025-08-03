using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands;
using Ordering.Core.Entities;
using Ordering.Core.Repositories;

namespace Ordering.Application.Handlers
{
    public class CheckoutOrderCommandHandlerV2 : IRequestHandler<CheckoutOrderCommandV2, int>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CheckoutOrderCommandHandlerV2> _logger;

        public CheckoutOrderCommandHandlerV2(IOrderRepository orderRepository, IMapper mapper, ILogger<CheckoutOrderCommandHandlerV2> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<int> Handle(CheckoutOrderCommandV2 request, CancellationToken cancellationToken)
        {
            var orderEntity = _mapper.Map<Order>(request);
            var generatedOrder = await _orderRepository.AddSync(orderEntity);
            _logger.LogInformation($"Order with Id {generatedOrder.Id} successfully created with V2 Handler");
            return generatedOrder.Id;
        }
    }
}
