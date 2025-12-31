using Core.Interfaces.Gateways;

namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        public IPagamentoGateway PagamentoRepository { get; }
    }
}
