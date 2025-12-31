using Core.Entities;
using Core.Enums;

namespace Core.Interfaces.Gateways
{
    public interface IPagamentoGateway
    {
        public Task<Pagamento> InsertPagamento(Pagamento pagamento);
        public Task UpdatePagamento(Pagamento pagamento);
        public Task<Pagamento?> GetById(int idPagamento);
        public Task<Pagamento?> GetByPedidoId(int idPedido);
        public Task<IEnumerable<Pagamento>> GetAllPagamentos(StatusPagamentoEnum? statusPagamentoEnum);
    }
}
