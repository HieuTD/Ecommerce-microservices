using MediatR;

namespace Basket.Application.Commands
{
    public class DeleteShoppingCartCommand : IRequest<Unit>
    {
        public string UserName { get; set; }
        public DeleteShoppingCartCommand(string userName)
        {
            UserName = userName;
        }
    }
}
