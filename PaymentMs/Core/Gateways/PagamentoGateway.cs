using Core.Interfaces.Gateways;
using Core.Interfaces;
using Core.Entities;
using Core.Enums;

namespace Core.Gateways
{
    public class PagamentoGateway(IDbConnection dbConnection) : IPagamentoGateway
    {
        private readonly IDbConnection _dbConnection = dbConnection;
        private readonly string _tableName = nameof(Pagamento);

        public async Task<Pagamento> InsertPagamento(Pagamento pagamento)
        {
            int insertedId = await _dbConnection.InsertAndReturnIdAsync(
                _tableName,
                new Dictionary<string, object>
                {
                    { "id_pedido", pagamento.IdPedido },
                    { "id_gateway_pagamento", pagamento.IdGatewayPagamento },
                    { "valor", pagamento.Valor },
                    { "id_status_pagamento", pagamento.IdStatusPagamento},
                    { "data_hora_pago", pagamento.DataHoraPago }
                }, "id_pagamento"
            );

            return await GetById(insertedId) ?? throw new Exception("Insert failed");
        }

        public async Task<Pagamento?> GetById(int idPagamento)
        {
            return await _dbConnection.SearchFirstOrDefaultByParametersAsync<Pagamento>(
                _tableName,
                "id_pagamento = @Id",
                new { Id = idPagamento }
            );
        }

        public async Task UpdatePagamento(Pagamento pagamento)
        {
            await _dbConnection.UpdateAsync(_tableName, new Dictionary<string, object>
            {
                { "id_gateway_pagamento", pagamento.IdGatewayPagamento },
                { "valor", pagamento.Valor },
                { "id_status_pagamento", pagamento.IdStatusPagamento },
                { "data_hora_pago", pagamento.DataHoraPago }
            }, "id_pagamento = @Id",
                new { Id = pagamento.IdPagamento });
        }

        public async Task<Pagamento?> GetByPedidoId(int idPedido)
        {
            return await _dbConnection.SearchFirstOrDefaultByParametersAsync<Pagamento>(
                _tableName,
                "id_pedido = @Id",
                new { Id = idPedido }
            );
        }

        public async Task<IEnumerable<Pagamento>> GetAllPagamentos(StatusPagamentoEnum? statusPagamentoEnum)
        {
            if (statusPagamentoEnum == null)
            {
                return await _dbConnection.ListAllAsync<Pagamento>(
                    _tableName
                );
            }
            else
            {
                return await _dbConnection.SearchByParametersAsync<Pagamento>(
                    _tableName,
                    "id_status_pagamento = @Status",
                    new { Status = (int)statusPagamentoEnum }
                );
            }
        }
    }
}