using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Services
{
    public class HistoricoPrecoService : IHistoricoPrecoService
    {
        private readonly IHistoricoPrecoRepository _historicoPrecoRepository;
        private readonly IEncomendaRepository _encomendaRepository;
        private readonly IArtigoRepository _artigoRepository;

        public HistoricoPrecoService(
            IHistoricoPrecoRepository historicoPrecoRepository,
            IEncomendaRepository encomendaRepository,
            IArtigoRepository artigoRepository)
        {
            _historicoPrecoRepository = historicoPrecoRepository;
            _encomendaRepository = encomendaRepository;
            _artigoRepository = artigoRepository;
        }

        // ══════════════════════════════════════════════════════════════════════
        // CONSULTAS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<List<HistoricoPrecoDto>>> ObterTodosAsync()
        {
            var historicos = await _historicoPrecoRepository.ObterTodosAsync();
            var dtos = historicos.Select(MapearParaDto).ToList();
            return Result<List<HistoricoPrecoDto>>.Ok(dtos);
        }

        public async Task<Result<HistoricoPrecoDto>> ObterPorIdAsync(int id)
        {
            var historico = await _historicoPrecoRepository.ObterPorIdAsync(id);

            if (historico == null)
                return Result<HistoricoPrecoDto>.Falhou("Histórico de preço não encontrado");

            return Result<HistoricoPrecoDto>.Ok(MapearParaDto(historico));
        }

        public async Task<Result<List<HistoricoPrecoDto>>> ObterPorArtigoAsync(int artigoId)
        {
            var historicos = await _historicoPrecoRepository.ObterPorArtigoAsync(artigoId);
            var dtos = historicos.Select(MapearParaDto).ToList();
            return Result<List<HistoricoPrecoDto>>.Ok(dtos);
        }

        public async Task<Result<List<HistoricoPrecoDto>>> ObterPorFornecedorAsync(int fornecedorId)
        {
            var historicos = await _historicoPrecoRepository.ObterPorFornecedorAsync(fornecedorId);
            var dtos = historicos.Select(MapearParaDto).ToList();
            return Result<List<HistoricoPrecoDto>>.Ok(dtos);
        }

        public async Task<Result<List<HistoricoPrecoDto>>> ObterPorEncomendaAsync(int encomendaId)
        {
            var historicos = await _historicoPrecoRepository.ObterPorEncomendaAsync(encomendaId);
            var dtos = historicos.Select(MapearParaDto).ToList();
            return Result<List<HistoricoPrecoDto>>.Ok(dtos);
        }

        public async Task<Result<List<HistoricoPrecoDto>>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            if (dataInicio > dataFim)
                return Result<List<HistoricoPrecoDto>>.Falhou("Data de início deve ser anterior à data de fim");

            var historicos = await _historicoPrecoRepository.ObterPorPeriodoAsync(dataInicio, dataFim);
            var dtos = historicos.Select(MapearParaDto).ToList();
            return Result<List<HistoricoPrecoDto>>.Ok(dtos);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ANÁLISES
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<List<EvolucaoPrecoDto>>> ObterEvolucaoPrecosAsync(int artigoId)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);
            if (artigo == null)
                return Result<List<EvolucaoPrecoDto>>.Falhou("Artigo não encontrado");

            var historicos = await _historicoPrecoRepository.ObterPorArtigoAsync(artigoId);

            if (!historicos.Any())
                return Result<List<EvolucaoPrecoDto>>.Ok(new List<EvolucaoPrecoDto>());

            var evolucoes = new List<EvolucaoPrecoDto>();

            // Agrupar por fornecedor e calcular evolução
            var gruposPorFornecedor = historicos.GroupBy(h => h.FornecedorId);

            foreach (var grupo in gruposPorFornecedor)
            {
                var ordenados = grupo.OrderByDescending(h => h.DataCompra).ToList();

                if (ordenados.Count >= 2)
                {
                    var atual = ordenados[0];
                    var anterior = ordenados[1];

                    var diferencaAbsoluta = atual.PrecoUnitario - anterior.PrecoUnitario;
                    var diferencaPercentual = anterior.PrecoUnitario > 0
                        ? (diferencaAbsoluta / anterior.PrecoUnitario) * 100
                        : 0;

                    evolucoes.Add(new EvolucaoPrecoDto(
                        artigoId,
                        artigo.Nome,
                        artigo.SKU,
                        atual.PrecoUnitario,
                        anterior.PrecoUnitario,
                        diferencaAbsoluta,
                        diferencaPercentual,
                        atual.DataCompra,
                        atual.Fornecedor.Nome
                    ));
                }
            }

            return Result<List<EvolucaoPrecoDto>>.Ok(evolucoes);
        }

        public async Task<Result<ComparacaoFornecedorDto>> CompararFornecedoresAsync(int artigoId)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);
            if (artigo == null)
                return Result<ComparacaoFornecedorDto>.Falhou("Artigo não encontrado");

            var historicos = await _historicoPrecoRepository.ObterPorArtigoAsync(artigoId);

            if (!historicos.Any())
            {
                var comparacaoVazia = new ComparacaoFornecedorDto(
                    artigoId,
                    artigo.Nome,
                    new List<PrecoFornecedorDto>()
                );
                return Result<ComparacaoFornecedorDto>.Ok(comparacaoVazia);
            }

            // Agrupar por fornecedor
            var gruposPorFornecedor = historicos.GroupBy(h => h.FornecedorId);

            var precosFornecedores = new List<PrecoFornecedorDto>();

            foreach (var grupo in gruposPorFornecedor)
            {
                var fornecedor = grupo.First().Fornecedor;
                var ordenados = grupo.OrderByDescending(h => h.DataCompra).ToList();
                var ultimoPreco = ordenados.First().PrecoUnitario;
                var precoMedio = grupo.Average(h => h.PrecoUnitario);
                var ultimaCompra = ordenados.First().DataCompra;
                var totalCompras = grupo.Count();

                precosFornecedores.Add(new PrecoFornecedorDto(
                    fornecedor.Id,
                    fornecedor.Nome,
                    precoMedio,
                    ultimoPreco,
                    ultimaCompra,
                    totalCompras
                ));
            }

            // Ordenar por último preço (mais barato primeiro)
            precosFornecedores = precosFornecedores.OrderBy(p => p.UltimoPreco).ToList();

            var comparacao = new ComparacaoFornecedorDto(
                artigoId,
                artigo.Nome,
                precosFornecedores
            );

            return Result<ComparacaoFornecedorDto>.Ok(comparacao);
        }

        public async Task<Result<List<SugestaoPrecoDto>>> ObterSugestoesPrecosAsync(List<int> artigosIds, int? fornecedorId = null)
        {
            var sugestoes = new List<SugestaoPrecoDto>();

            foreach (var artigoId in artigosIds)
            {
                var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);
                if (artigo == null)
                    continue;

                // Tentar obter último preço deste fornecedor específico
                var ultimoPreco = await _historicoPrecoRepository.ObterUltimoPrecoAsync(artigoId, fornecedorId);

                // Se não houver histórico com este fornecedor, buscar de qualquer fornecedor
                if (ultimoPreco == null && fornecedorId.HasValue)
                {
                    ultimoPreco = await _historicoPrecoRepository.ObterUltimoPrecoAsync(artigoId, null);
                }

                var precoMedio = await _historicoPrecoRepository.ObterPrecoMedioAsync(artigoId, fornecedorId);

                var sugestao = new SugestaoPrecoDto(
                    artigoId,
                    artigo.Nome,
                    ultimoPreco?.FornecedorId,
                    ultimoPreco?.Fornecedor.Nome,
                    ultimoPreco?.PrecoUnitario,
                    precoMedio ?? artigo.PrecoMedio,
                    ultimoPreco?.DataCompra,
                    ultimoPreco != null
                );

                sugestoes.Add(sugestao);
            }

            return Result<List<SugestaoPrecoDto>>.Ok(sugestoes);
        }

        // ══════════════════════════════════════════════════════════════════════
        // OPERAÇÕES (Chamadas automaticamente pela Receção)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<bool>> RegistarHistoricoDeEncomendaAsync(int encomendaId, string emailUtilizador)
        {
            var encomenda = await _encomendaRepository.ObterPorIdAsync(encomendaId);

            if (encomenda == null)
                return Result<bool>.Falhou("Encomenda não encontrada");

            if (encomenda.Estado != Domain.Enums.EstadoEncomenda.CONCLUIDA)
                return Result<bool>.Falhou("Só é possível registar histórico de encomendas concluídas");

            if (!encomenda.FornecedorId.HasValue)
                return Result<bool>.Falhou("Encomenda não possui fornecedor definido");

            // Verificar se já existe histórico desta encomenda
            var historicoExistente = await _historicoPrecoRepository.ObterPorEncomendaAsync(encomendaId);
            if (historicoExistente.Any())
            {
                // Já foi registado, não duplicar
                return Result<bool>.Ok(true);
            }

            var historicos = new List<HistoricoPreco>();

            foreach (var linha in encomenda.Linhas)
            {
                // Só registar se tem preço e quantidade recebida
                if (!linha.PrecoUnitario.HasValue || linha.QuantidadeRecebida == 0)
                    continue;

                var historico = new HistoricoPreco
                {
                    ArtigoId = linha.ArtigoId,
                    FornecedorId = encomenda.FornecedorId.Value,
                    EncomendaId = encomendaId,
                    PrecoUnitario = linha.PrecoUnitario.Value,
                    Quantidade = linha.QuantidadeRecebida,
                    ValorTotal = linha.PrecoUnitario.Value * linha.QuantidadeRecebida,
                    DataCompra = encomenda.DataEntregaReal ?? DateTime.UtcNow,
                    Observacoes = !string.IsNullOrEmpty(linha.Observacoes)
                        ? $"Encomenda {encomenda.NumeroEncomenda}: {linha.Observacoes}"
                        : $"Encomenda {encomenda.NumeroEncomenda}",
                    CriadoEm = DateTime.UtcNow,
                    CriadoPor = emailUtilizador
                };

                historicos.Add(historico);
            }

            if (historicos.Any())
            {
                await _historicoPrecoRepository.AdicionarVariosAsync(historicos);
                await _historicoPrecoRepository.SaveChangesAsync();
            }

            return Result<bool>.Ok(true);
        }

        // ══════════════════════════════════════════════════════════════════════
        // MAPEAMENTO
        // ══════════════════════════════════════════════════════════════════════

        private static HistoricoPrecoDto MapearParaDto(HistoricoPreco hp)
        {
            return new HistoricoPrecoDto(
                hp.Id,
                hp.ArtigoId,
                hp.Artigo?.Nome ?? "N/A",
                hp.Artigo?.SKU ?? "N/A",
                hp.FornecedorId,
                hp.Fornecedor?.Nome ?? "N/A",
                hp.EncomendaId,
                hp.Encomenda?.NumeroEncomenda ?? "N/A",
                hp.PrecoUnitario,
                hp.Quantidade,
                hp.ValorTotal,
                hp.DataCompra,
                hp.Observacoes,
                hp.CriadoEm,
                hp.CriadoPor
            );
        }
    }
}