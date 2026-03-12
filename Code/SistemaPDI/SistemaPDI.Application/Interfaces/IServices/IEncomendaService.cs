using Microsoft.AspNetCore.Http;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Interfaces.IServices
{
    public interface IEncomendaService
    {
        // ══════════════════════════════════════════════════════════════════════
        // CONSULTAS
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<List<EncomendaDto>>> ObterTodosAsync(bool incluirInativos = false);
        Task<Result<EncomendaDto>> ObterPorIdAsync(int id);
        Task<Result<List<EncomendaDto>>> ObterPorEstadoAsync(string estado);
        Task<Result<List<EncomendaDto>>> ObterMinhasEncomendasAsync(string emailUtilizador);
        Task<Result<List<EncomendaDto>>> ObterPendentesAprovacaoAsync(); // Para GESTOR_FINANCEIRO
        Task<Result<List<EncomendaDto>>> ObterEncomendasEnviadasAsync(); // Para receção
        Task<Result<List<EncomendaDropdownDto>>> ObterPendentesParaDropdownAsync();

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 1: CRIAR LISTA (Estado: LISTA)
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<EncomendaDto>> CriarListaAsync(CriarListaDto dto, string emailUtilizador);
        Task<Result<EncomendaDto>> AtualizarListaAsync(int id, CriarListaDto dto);

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 2: GERAR PDF E ENVIAR (LISTA → RASCUNHO)
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<byte[]>> GerarPdfListaAsync(int id);
        Task<Result<EncomendaDto>> MarcarComoRascunhoAsync(int id, string emailUtilizador);

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 3: SUBMETER ORÇAMENTO (RASCUNHO → PENDENTE)
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<EncomendaDto>> SubmeterOrcamentoAsync(
            int id,
            SubmeterOrcamentoDto dto,
            IFormFile orcamentoPdf,
            string emailUtilizador);
        Task<Result<EncomendaDto>> SubmeterOrcamentoSimplesAsync(int id, SubmeterOrcamentoDto dto, string emailUtilizador);

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 4: REJEITAR (PENDENTE → LISTA)
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<EncomendaDto>> RejeitarAsync(int id, string emailGestorLogistica, string motivo);

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 5: APROVAR E PREENCHER (PENDENTE → CONFIRMADA)
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<EncomendaDto>> AprovarEPreencherAsync(
            int id,
            AprovarEPreencherDto dto,
            string emailGestorLogistica);

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 6: CONFIRMAR E ENVIAR (CONFIRMADA → ENVIADA)
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<EncomendaDto>> ConfirmarEEnviarAsync(int id, string emailGestorLogistica);

        // ══════════════════════════════════════════════════════════════════════
        // ETAPA 7: REGISTAR RECEÇÃO (ENVIADA → PARCIAL/CONCLUIDA)
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<EncomendaDto>> RegistarRecepcaoAsync(
            RegistarRecepcaoDto dto,
            string emailUtilizador);

        // ══════════════════════════════════════════════════════════════════════
        // OUTRAS OPERAÇÕES
        // ══════════════════════════════════════════════════════════════════════

        Task<Result<bool>> CancelarAsync(int id, string motivo);
        Task<Result<bool>> AlterarEstadoAsync(int id, bool ativo);
    }
}