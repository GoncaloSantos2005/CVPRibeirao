using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Services
{
    /// <summary>
    /// Serviço de gestão de lotes.
    /// Implementa as regras de negócio RN03, RN04, RN05, RN06, RN13, RN17.
    /// </summary>
    public class LoteService : ILoteService
    {
        private readonly ILoteRepository _loteRepository;
        private readonly IArtigoRepository _artigoRepository;

        public LoteService(
            ILoteRepository loteRepository,
            IArtigoRepository artigoRepository)
        {
            _loteRepository = loteRepository;
            _artigoRepository = artigoRepository;
        }

        // ══════════════════════════════════════════════════════════════════════
        // LEITURA
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Obtém todos os lotes ativos.
        /// </summary>
        public async Task<Result<List<LoteDto>>> ObterTodosAsync()
        {
            var lotes = await _loteRepository.ObterTodosAsync();
            var dtos = lotes.Select(MapToDto).ToList();
            return Result<List<LoteDto>>.Ok(dtos);
        }

        /// <summary>
        /// Obtém um lote por ID.
        /// </summary>
        public async Task<Result<LoteDto>> ObterPorIdAsync(int id)
        {
            var lote = await _loteRepository.ObterPorIdComArtigoAsync(id);

            if (lote == null)
                return Result<LoteDto>.Falhou("Lote não encontrado.");

            return Result<LoteDto>.Ok(MapToDto(lote));
        }

        /// <summary>
        /// Obtém todos os lotes de um artigo.
        /// </summary>
        public async Task<Result<List<LoteDto>>> ObterPorArtigoAsync(int artigoId)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);
            if (artigo == null)
                return Result<List<LoteDto>>.Falhou("Artigo não encontrado.");

            var lotes = await _loteRepository.ObterPorArtigoIdAsync(artigoId);
            var dtos = lotes.Select(MapToDto).ToList();
            return Result<List<LoteDto>>.Ok(dtos);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ESCRITA
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Cria um novo lote durante a receção de stock.
        /// Valida RN06 (duplicado) e RN17 (data validade).
        /// </summary>
        public async Task<Result<LoteDto>> CriarAsync(CriarLoteDto dto)
        {
            // Validar artigo existe
            var artigo = await _artigoRepository.ObterPorIdAsync(dto.ArtigoId);
            if (artigo == null)
                return Result<LoteDto>.Falhou("Artigo não encontrado.");

            // RN17 - Validação de Data de Validade
            if (dto.DataValidade.Date < DateTime.UtcNow.Date)
                return Result<LoteDto>.Falhou("A data de validade não pode ser inferior à data atual.");

            // RN06 - Validação de Lote Duplicado
            if (await _loteRepository.NumeroLoteJaExisteAsync(dto.ArtigoId, dto.NumeroLote))
                return Result<LoteDto>.Falhou($"Já existe um lote com o número '{dto.NumeroLote}' para este artigo.");

            // Validar quantidade
            if (dto.Quantidade <= 0)
                return Result<LoteDto>.Falhou("A quantidade deve ser maior que zero.");

            // Validar preço
            if (dto.PrecoUnitario < 0)
                return Result<LoteDto>.Falhou("O preço unitário não pode ser negativo.");

            var lote = new Lote
            {
                ArtigoId = dto.ArtigoId,
                NumeroLote = dto.NumeroLote.Trim().ToUpper(),
                DataValidade = dto.DataValidade.Date,
                PrecoUnitario = dto.PrecoUnitario,
                QtdDisponivel = dto.Quantidade,
                QtdReservada = 0,
                LocalizacaoId = dto.LocalizacaoId,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            await _loteRepository.AdicionarAsync(lote);

            // Atualizar stock do artigo
            artigo.StockFisico += dto.Quantidade;
            artigo.AtualizarPrecoMedio(dto.Quantidade, dto.PrecoUnitario);
            artigo.AtualizadoEm = DateTime.UtcNow;

            await _loteRepository.SaveChangesAsync();

            // Recarregar lote com artigo para o DTO
            var loteCompleto = await _loteRepository.ObterPorIdComArtigoAsync(lote.Id);
            return Result<LoteDto>.Ok(MapToDto(loteCompleto!));
        }

        /// <summary>
        /// Atualiza um lote existente.
        /// </summary>
        public async Task<Result<LoteDto>> AtualizarAsync(int id, AtualizarLoteDto dto)
        {
            var lote = await _loteRepository.ObterPorIdComArtigoAsync(id);

            if (lote == null)
                return Result<LoteDto>.Falhou("Lote não encontrado.");

            // Validar quantidade (não pode ser menor que a reservada)
            if (dto.QtdDisponivel < lote.QtdReservada)
                return Result<LoteDto>.Falhou($"A quantidade disponível não pode ser inferior à quantidade reservada ({lote.QtdReservada}).");

            var diferencaStock = dto.QtdDisponivel - lote.QtdDisponivel;

            lote.LocalizacaoId = dto.LocalizacaoId;
            lote.QtdDisponivel = dto.QtdDisponivel;
            lote.DataValidade = dto.DataValidade;
            lote.PrecoUnitario = dto.PrecoUnitario;

            // Atualizar stock do artigo
            lote.Artigo.StockFisico += diferencaStock;
            lote.Artigo.AtualizadoEm = DateTime.UtcNow;

            await _loteRepository.AtualizarAsync(lote);
            await _loteRepository.SaveChangesAsync();

            // Recarregar para ter a Localizacao atualizada
            var loteAtualizado = await _loteRepository.ObterPorIdComArtigoAsync(id);
            return Result<LoteDto>.Ok(MapToDto(loteAtualizado!));
        }

        /// <summary>
        /// Desativa um lote.
        /// </summary>
        public async Task<Result> DesativarAsync(int id)
        {
            var lote = await _loteRepository.ObterPorIdComArtigoAsync(id);

            if (lote == null)
                return Result.Falhou("Lote não encontrado.");

            if (lote.QtdReservada > 0)
                return Result.Falhou("Não é possível desativar um lote com reservas ativas.");

            // Atualizar stock do artigo
            lote.Artigo.StockFisico -= lote.QtdDisponivel;
            lote.Artigo.AtualizadoEm = DateTime.UtcNow;

            lote.Ativo = false;
            lote.QtdDisponivel = 0;

            await _loteRepository.AtualizarAsync(lote);
            await _loteRepository.SaveChangesAsync();

            return Result.Ok();
        }

        // ══════════════════════════════════════════════════════════════════════
        // FEFO - RESERVAS (RN03, RN04, RN05)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Reserva stock usando algoritmo FEFO (RN03, RN04).
        /// Aloca de múltiplos lotes se necessário, ordenados por validade.
        /// </summary>
        public async Task<Result<ResultadoReservaDto>> ReservarStockFEFOAsync(ReservaStockDto dto)
        {
            if (dto.QuantidadeTotal <= 0)
                return Result<ResultadoReservaDto>.Falhou("A quantidade deve ser maior que zero.");

            // Obter lotes disponíveis ordenados por FEFO (primeiro a expirar)
            var lotesDisponiveis = await _loteRepository.ObterLotesParaFEFOAsync(dto.ArtigoId);

            if (!lotesDisponiveis.Any())
                return Result<ResultadoReservaDto>.Falhou("Não existem lotes disponíveis para este artigo.");

            // Calcular stock total disponível
            var stockTotalDisponivel = lotesDisponiveis.Sum(l => l.QtdRealmenteDisponivel);

            if (stockTotalDisponivel < dto.QuantidadeTotal)
            {
                return Result<ResultadoReservaDto>.Falhou(
                    $"Stock insuficiente. Disponível: {stockTotalDisponivel}, Solicitado: {dto.QuantidadeTotal}");
            }

            // Alocar usando FEFO
            var alocacoes = new List<AlocacaoLoteDto>();
            var lotesAlterados = new List<Lote>();
            var quantidadeRestante = dto.QuantidadeTotal;

            foreach (var lote in lotesDisponiveis)
            {
                if (quantidadeRestante <= 0)
                    break;

                var qtdParaAlocar = Math.Min(quantidadeRestante, lote.QtdRealmenteDisponivel);

                if (qtdParaAlocar > 0 && lote.Reservar(qtdParaAlocar))
                {
                    alocacoes.Add(new AlocacaoLoteDto(
                        lote.Id,
                        lote.NumeroLote ?? string.Empty,
                        lote.DataValidade ?? DateTime.MinValue,
                        qtdParaAlocar,
                        FormatarLocalizacao(lote.Localizacao)
                    ));

                    lotesAlterados.Add(lote);
                    quantidadeRestante -= qtdParaAlocar;
                }
            }

            // Persistir alterações
            await _loteRepository.AtualizarVariosAsync(lotesAlterados);
            await _loteRepository.SaveChangesAsync();

            var resultado = new ResultadoReservaDto(
                dto.ArtigoId,
                dto.QuantidadeTotal,
                dto.QuantidadeTotal - quantidadeRestante,
                quantidadeRestante == 0,
                alocacoes
            );

            return Result<ResultadoReservaDto>.Ok(resultado);
        }

        /// <summary>
        /// Liberta reservas de um lote específico (RN05 - timeout/cancelamento).
        /// </summary>
        public async Task<Result> LibertarReservaAsync(LibertarReservaDto dto)
        {
            var lote = await _loteRepository.ObterPorIdAsync(dto.LoteId);

            if (lote == null)
                return Result.Falhou("Lote não encontrado.");

            if (dto.Quantidade <= 0)
                return Result.Falhou("A quantidade deve ser maior que zero.");

            if (dto.Quantidade > lote.QtdReservada)
                return Result.Falhou($"Quantidade a libertar ({dto.Quantidade}) excede a quantidade reservada ({lote.QtdReservada}).");

            if (!lote.LibertarReserva(dto.Quantidade))
                return Result.Falhou("Não foi possível libertar a reserva.");

            await _loteRepository.AtualizarAsync(lote);
            await _loteRepository.SaveChangesAsync();

            return Result.Ok();
        }

        /// <summary>
        /// Liberta todas as reservas expiradas (job automático - RN05).
        /// Nota: Requer integração com GuiaPicking para verificar timeout de 2h.
        /// </summary>
        public async Task<Result<int>> LibertarReservasExpiradasAsync()
        {
            // Por agora, retorna 0 - será implementado com GuiaPicking
            // A lógica completa verificará:
            // 1. Guias de Picking com mais de 2 horas sem confirmação
            // 2. Liberta as reservas associadas a essas guias

            var lotesComReservas = await _loteRepository.ObterLotesComReservasExpiradasAsync();
            
            // TODO: Implementar lógica de timeout com GuiaPicking
            // Por agora, apenas conta os lotes que precisam de verificação

            return Result<int>.Ok(0);
        }

        /// <summary>
        /// Confirma a saída de stock (converte reservas em saídas definitivas).
        /// </summary>
        public async Task<Result> ConfirmarSaidaAsync(int loteId, int quantidade)
        {
            var lote = await _loteRepository.ObterPorIdComArtigoAsync(loteId);

            if (lote == null)
                return Result.Falhou("Lote não encontrado.");

            if (quantidade <= 0)
                return Result.Falhou("A quantidade deve ser maior que zero.");

            if (quantidade > lote.QtdReservada)
                return Result.Falhou($"Quantidade a confirmar ({quantidade}) excede a quantidade reservada ({lote.QtdReservada}).");

            if (!lote.ConfirmarSaida(quantidade))
                return Result.Falhou("Não foi possível confirmar a saída.");

            // Atualizar stock do artigo
            lote.Artigo.StockFisico -= quantidade;
            lote.Artigo.AtualizadoEm = DateTime.UtcNow;

            await _loteRepository.AtualizarAsync(lote);
            await _loteRepository.SaveChangesAsync();

            return Result.Ok();
        }

        // ══════════════════════════════════════════════════════════════════════
        // STOCK
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Calcula o stock total disponível de um artigo (soma de todos os lotes).
        /// </summary>
        public async Task<Result<int>> CalcularStockDisponivelAsync(int artigoId)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);
            if (artigo == null)
                return Result<int>.Falhou("Artigo não encontrado.");

            var lotes = await _loteRepository.ObterLotesParaFEFOAsync(artigoId);
            var stockTotal = lotes.Sum(l => l.QtdRealmenteDisponivel);

            return Result<int>.Ok(stockTotal);
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Mapeia uma entidade Lote para DTO.
        /// </summary>
        private static LoteDto MapToDto(Lote l) => new(
            l.Id,
            l.ArtigoId,
            l.Artigo?.Nome ?? string.Empty,
            l.Artigo?.SKU ?? string.Empty,
            l.Artigo?.UrlImagem,
            l.NumeroLote ?? string.Empty,
            l.DataValidade ?? DateTime.MinValue,
            l.PrecoUnitario ?? 0m,
            l.QtdDisponivel,
            l.QtdReservada,
            l.QtdRealmenteDisponivel,
            l.LocalizacaoId,
            FormatarLocalizacao(l.Localizacao),
            l.Ativo,
            l.EstaExpirado,
            l.ValidadeProxima,
            l.CriadoEm,
            l.EmTrafico
        );

        /// <summary>
        /// Formata a localização para exibição usando o Label calculado.
        /// Ex: "PRATELEIRA_A_3_2"
        /// </summary>
        private static string? FormatarLocalizacao(Localizacao? loc)
        {
            return loc?.Label;
        }
    }
}