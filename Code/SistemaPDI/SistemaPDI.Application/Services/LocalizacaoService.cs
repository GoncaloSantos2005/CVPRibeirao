using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Domain.Enums;

namespace SistemaPDI.Application.Services
{
    public class LocalizacaoService : ILocalizacaoService
    {
        private readonly ILocalizacaoRepository _localizacaoRepository;

        public LocalizacaoService(ILocalizacaoRepository localizacaoRepository)
        {
            _localizacaoRepository = localizacaoRepository;
        }

        // ══════════════════════════════════════════════════════════════════════
        // LEITURA
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<List<LocalizacaoDto>>> ObterTodosAsync(bool incluirInativos = false)
        {
            var localizacoes = await _localizacaoRepository.ObterTodosAsync(incluirInativos);
            var dtos = localizacoes.Select(MapearParaDto).ToList();
            return Result<List<LocalizacaoDto>>.Ok(dtos);
        }

        public async Task<Result<List<LocalizacaoDto>>> ObterAtivasAsync()
        {
            var localizacoes = await _localizacaoRepository.ObterAtivasAsync();
            var dtos = localizacoes.Select(MapearParaDto).ToList();
            return Result<List<LocalizacaoDto>>.Ok(dtos);
        }

        public async Task<Result<LocalizacaoDto>> ObterPorIdAsync(int id)
        {
            var localizacao = await _localizacaoRepository.ObterPorIdAsync(id);

            if (localizacao == null)
                return Result<LocalizacaoDto>.Falhou("Localização não encontrada.");

            return Result<LocalizacaoDto>.Ok(MapearParaDto(localizacao));
        }

        public async Task<Result<List<LocalizacaoDto>>> ObterPorTipoAsync(TipoLocalizacao tipo)
        {
            var localizacoes = await _localizacaoRepository.ObterPorTipoAsync(tipo);
            var dtos = localizacoes.Select(MapearParaDto).ToList();
            return Result<List<LocalizacaoDto>>.Ok(dtos);
        }

        public async Task<Result<List<LocalizacaoDto>>> ObterPorZonaAsync(string zona)
        {
            if (string.IsNullOrWhiteSpace(zona))
                return Result<List<LocalizacaoDto>>.Falhou("A zona é obrigatória.");

            var localizacoes = await _localizacaoRepository.ObterPorZonaAsync(zona);
            var dtos = localizacoes.Select(MapearParaDto).ToList();
            return Result<List<LocalizacaoDto>>.Ok(dtos);
        }

        public async Task<Result<List<LocalizacaoDropdownDto>>> ObterParaDropdownAsync()
        {
            var localizacoes = await _localizacaoRepository.ObterAtivasAsync();

            var dtos = localizacoes
                .OrderBy(l => l.Label)
                .Select(l => new LocalizacaoDropdownDto(l.Id, l.Label))
                .ToList();

            return Result<List<LocalizacaoDropdownDto>>.Ok(dtos);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ESCRITA
        // ══════════════════════════════════════════════════════════════════════

        public async Task<Result<LocalizacaoDto>> CriarAsync(CriarLocalizacaoDto dto)
        {
            // Validações
            if (string.IsNullOrWhiteSpace(dto.Codigo))
                return Result<LocalizacaoDto>.Falhou("O código é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Zona))
                return Result<LocalizacaoDto>.Falhou("A zona é obrigatória.");

            if (await _localizacaoRepository.CodigoJaExisteAsync(dto.Codigo.Trim()))
                return Result<LocalizacaoDto>.Falhou("Já existe uma localização com este código.");

            // Verificar se label já existe (combinação tipo+zona+prateleira+nivel)
            if (await _localizacaoRepository.LabelJaExisteAsync(dto.Tipo, dto.Zona.Trim(), dto.Prateleira?.Trim(), dto.Nivel?.Trim()))
                return Result<LocalizacaoDto>.Falhou("Já existe uma localização com esta combinação (Tipo/Zona/Prateleira/Nível).");

            var localizacao = new Localizacao
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Tipo = dto.Tipo,
                Zona = dto.Zona.Trim().ToUpper(),
                Prateleira = dto.Prateleira?.Trim().ToUpper(),
                Nivel = dto.Nivel?.Trim().ToUpper(),
                Observacoes = dto.Observacoes?.Trim(),
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            await _localizacaoRepository.AdicionarAsync(localizacao);
            await _localizacaoRepository.SaveChangesAsync();

            return Result<LocalizacaoDto>.Ok(MapearParaDto(localizacao));
        }

        public async Task<Result<LocalizacaoDto>> AtualizarAsync(int id, AtualizarLocalizacaoDto dto)
        {
            var localizacao = await _localizacaoRepository.ObterPorIdAsync(id);

            if (localizacao == null)
                return Result<LocalizacaoDto>.Falhou("Localização não encontrada.");

            // Validações
            if (string.IsNullOrWhiteSpace(dto.Codigo))
                return Result<LocalizacaoDto>.Falhou("O código é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Zona))
                return Result<LocalizacaoDto>.Falhou("A zona é obrigatória.");

            if (await _localizacaoRepository.CodigoJaExisteAsync(dto.Codigo.Trim(), id))
                return Result<LocalizacaoDto>.Falhou("Já existe outra localização com este código.");

            // Verificar se label já existe
            if (await _localizacaoRepository.LabelJaExisteAsync(dto.Tipo, dto.Zona.Trim(), dto.Prateleira?.Trim(), dto.Nivel?.Trim(), id))
                return Result<LocalizacaoDto>.Falhou("Já existe outra localização com esta combinação (Tipo/Zona/Prateleira/Nível).");

            // Atualizar campos
            localizacao.Codigo = dto.Codigo.Trim().ToUpper();
            localizacao.Tipo = dto.Tipo;
            localizacao.Zona = dto.Zona.Trim().ToUpper();
            localizacao.Prateleira = dto.Prateleira?.Trim().ToUpper();
            localizacao.Nivel = dto.Nivel?.Trim().ToUpper();
            localizacao.Observacoes = dto.Observacoes?.Trim();

            await _localizacaoRepository.AtualizarAsync(localizacao);
            await _localizacaoRepository.SaveChangesAsync();

            return Result<LocalizacaoDto>.Ok(MapearParaDto(localizacao));
        }

        public async Task<Result> AlternarEstadoAtivoAsync(int id)
        {
            var localizacao = await _localizacaoRepository.ObterPorIdAsync(id);

            if (localizacao == null)
                return Result.Falhou("Localização não encontrada.");

            // Se for desativar, verificar se tem lotes
            if (localizacao.Ativo)
            {
                var totalLotes = await _localizacaoRepository.ContarLotesAsync(id);
                if (totalLotes > 0)
                    return Result.Falhou($"Não é possível desativar. Localização tem {totalLotes} lote(s) associado(s).");
            }

            localizacao.Ativo = !localizacao.Ativo;

            await _localizacaoRepository.AtualizarAsync(localizacao);
            await _localizacaoRepository.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> ApagarAsync(int id)
        {
            var localizacao = await _localizacaoRepository.ObterPorIdAsync(id);

            if (localizacao == null)
                return Result.Falhou("Localização não encontrada.");

            // Verificar se tem lotes
            var totalLotes = await _localizacaoRepository.ContarLotesAsync(id);
            if (totalLotes > 0)
                return Result.Falhou($"Não é possível apagar. Localização tem {totalLotes} lote(s) associado(s). Desative-a em vez de apagar.");

            // Soft delete
            localizacao.Ativo = false;
            await _localizacaoRepository.AtualizarAsync(localizacao);
            await _localizacaoRepository.SaveChangesAsync();

            return Result.Ok();
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════════════

        private static LocalizacaoDto MapearParaDto(Localizacao l)
        {
            return new LocalizacaoDto(
                l.Id,
                l.Codigo,
                l.Tipo,
                ObterNomeTipo(l.Tipo),
                l.Zona,
                l.Prateleira,
                l.Nivel,
                l.Observacoes,
                l.Label,    // Propriedade calculada
                l.Ativo,
                l.CriadoEm
            );
        }

        private static string ObterNomeTipo(TipoLocalizacao tipo)
        {
            return tipo switch
            {
                TipoLocalizacao.PRATELEIRA => "Prateleira",
                TipoLocalizacao.ARMARIO => "Armário",
                TipoLocalizacao.FRIGORIFICO => "Frigorífico",
                TipoLocalizacao.ZONA => "Zona",
                _ => tipo.ToString()
            };
        }
    }
}