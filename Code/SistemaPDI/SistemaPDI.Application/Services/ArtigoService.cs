using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Services
{
    public class ArtigoService : IArtigoService
    {
        private readonly IArtigoRepository _artigoRepository;

        public ArtigoService(IArtigoRepository artigoRepository)
        {
            _artigoRepository = artigoRepository;
        }

        // ── 1. LEITURA (GET) ──────────────────────────────────────────────────

        public async Task<Result<List<ArtigoDto>>> ObterTodosAsync()
        {
            // Retorna apenas artigos ativos
            var artigos = await _artigoRepository.ObterAtivosAsync();
            var dtos = artigos.Select(MapearParaDto).ToList();
            return Result<List<ArtigoDto>>.Ok(dtos);
        }

        public async Task<Result<ArtigoDto>> ObterPorIdAsync(int id)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(id);

            if (artigo == null)
                return Result<ArtigoDto>.Falhou($"O artigo com o ID {id} não foi encontrado.");

            return Result<ArtigoDto>.Ok(MapearParaDto(artigo));
        }

        // ── 2. ESCRITA (POST / PUT / PATCH) ───────────────────────────────────

        public async Task<Result<ArtigoDto>> CriarAsync(CriarArtigoDto dto)
        {
            var novoArtigo = new Artigo
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                SKU = dto.SKU,
                UrlImagem = dto.UrlImagem,
                CategoriaId = dto.CategoriaId,
                StockMinimo = dto.StockMinimo,
                StockCritico = dto.StockCritico
            };

            await _artigoRepository.AdicionarAsync(novoArtigo);
            await _artigoRepository.SaveChangesAsync();

            return Result<ArtigoDto>.Ok(MapearParaDto(novoArtigo));
        }

        public async Task<Result> AtualizarAsync(int id, AtualizarArtigoDto dto)
        {
            var artigoExistente = await _artigoRepository.ObterPorIdAsync(id);

            if (artigoExistente == null)
                return Result.Falhou("Artigo não encontrado para atualização.");

            artigoExistente.Nome = dto.Nome;
            artigoExistente.Descricao = dto.Descricao;
            artigoExistente.SKU = dto.SKU;
            artigoExistente.UrlImagem = dto.UrlImagem;
            artigoExistente.CategoriaId = dto.CategoriaId;
            artigoExistente.StockMinimo = dto.StockMinimo;
            artigoExistente.StockCritico = dto.StockCritico;
            artigoExistente.AtualizadoEm = DateTime.UtcNow;

            await _artigoRepository.AtualizarAsync(artigoExistente);
            await _artigoRepository.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> AlternarEstadoAtivoAsync(int id, string nomeUtilizador)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(id);

            if (artigo == null)
                return Result.Falhou("Artigo não encontrado.");

            _artigoRepository.ToggleAtivo(artigo, nomeUtilizador);
            await _artigoRepository.SaveChangesAsync();

            return Result.Ok();
        }

        // ── 3. STOCK ──────────────────────────────────────────────────────────

        public async Task<Result<int>> CalcularStockVirtualAsync(int artigoId)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);

            if (artigo == null)
                return Result<int>.Falhou("Artigo não encontrado.");

            artigo.RecalcularStockVirtual();

            return Result<int>.Ok(artigo.StockVirtual);
        }

        public async Task<Result> AtualizarStockFisicoAsync(int artigoId, int quantidade)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);

            if (artigo == null)
                return Result.Falhou("Artigo não encontrado para atualizar o stock.");

            artigo.StockFisico += quantidade;
            artigo.RecalcularStockVirtual();

            await _artigoRepository.AtualizarAsync(artigo);
            await _artigoRepository.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result<bool>> VerificarNecessidadeReposicaoAsync(int artigoId)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);
            if (artigo == null) return Result<bool>.Falhou("Artigo não encontrado.");

            return Result<bool>.Ok(artigo.NecessitaReposicao());
        }

        public async Task<Result<bool>> VerificarStockCriticoAsync(int artigoId)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);
            if (artigo == null) return Result<bool>.Falhou("Artigo não encontrado.");

            return Result<bool>.Ok(artigo.EstaCritico());
        }

        public async Task<Result<int>> SugerirQuantidadeEncomendaAsync(int artigoId)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);
            if (artigo == null) return Result<int>.Falhou("Artigo não encontrado.");

            return Result<int>.Ok(artigo.SugerirQuantidade());
        }

        public async Task<Result> AtualizarPrecoMedioAsync(int artigoId, int qtdEntrada, decimal precoEntrada)
        {
            var artigo = await _artigoRepository.ObterPorIdAsync(artigoId);
            if (artigo == null) return Result.Falhou("Artigo não encontrado.");

            artigo.AtualizarPrecoMedio(qtdEntrada, precoEntrada);

            await _artigoRepository.AtualizarAsync(artigo);
            await _artigoRepository.SaveChangesAsync();

            return Result.Ok();
        }

        // ── LISTAS FILTRADAS POR STOCK ────────────────────────────────────────────

        public async Task<Result<List<ArtigoDto>>> ObterComStockBaixoAsync()
        {
            var artigos = await _artigoRepository.ObterComStockBaixoAsync();
            var dtos = artigos.Select(MapearParaDto).ToList();
            return Result<List<ArtigoDto>>.Ok(dtos);
        }

        public async Task<Result<List<ArtigoDto>>> ObterComStockCriticoAsync()
        {
            var artigos = await _artigoRepository.ObterComStockCriticoAsync();
            var dtos = artigos.Select(MapearParaDto).ToList();
            return Result<List<ArtigoDto>>.Ok(dtos);
        }

        public async Task<Result<List<ArtigoDto>>> ObterDesativadosAsync()
        {
            var artigos = await _artigoRepository.ObterDesativadosAsync();
            var dtos = artigos.Select(MapearParaDto).ToList();
            return Result<List<ArtigoDto>>.Ok(dtos);
        }

        // ── 4. MÉTODO AUXILIAR DE MAPEAMENTO ──────────────────────────────────

        private static ArtigoDto MapearParaDto(Artigo artigo)
        {
            return new ArtigoDto(
                artigo.Id,
                artigo.Nome,
                artigo.Descricao,
                artigo.SKU,
                artigo.UrlImagem,
                artigo.Categoria?.Nome ?? "Sem CategoriaDtos",
                artigo.StockFisico,
                artigo.StockVirtual,
                artigo.StockPendente,
                artigo.StockMinimo,
                artigo.StockCritico,
                artigo.PrecoMedio,
                artigo.UltimoPreco,
                artigo.CriadoEm,
                artigo.Ativo,
                artigo.NecessitaReposicao(),
                artigo.EstaCritico(),
                artigo.CategoriaId,
                artigo.Categoria?.Nome
            );
        }
    }
}