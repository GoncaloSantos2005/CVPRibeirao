using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriaService(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        public async Task<Result<List<CategoriaDto>>> ObterTodosAsync(bool incluirInativos = false)
        {
            var categorias = await _categoriaRepository.ObterTodosAsync(incluirInativos);

            var dtos = new List<CategoriaDto>();
            foreach (var cat in categorias)
            {
                var totalArtigos = await _categoriaRepository.ContarArtigosPorCategoriaAsync(cat.Id);
                dtos.Add(new CategoriaDto(cat.Id, cat.Nome, cat.Descricao, cat.Ativo, totalArtigos));
            }

            return Result<List<CategoriaDto>>.Ok(dtos);
        }

        public async Task<Result<List<CategoriaDto>>> ObterAtivasAsync()
        {
            var categorias = await _categoriaRepository.ObterAtivasAsync();

            var dtos = categorias.Select(c => new CategoriaDto(
                c.Id,
                c.Nome,
                c.Descricao,
                c.Ativo,
                0 // Não precisa contar para dropdown
            )).ToList();

            return Result<List<CategoriaDto>>.Ok(dtos);
        }

        public async Task<Result<CategoriaDto>> ObterPorIdAsync(int id)
        {
            var categoria = await _categoriaRepository.ObterPorIdAsync(id);

            if (categoria == null)
                return Result<CategoriaDto>.Falhou("Categoria não encontrada");

            var totalArtigos = await _categoriaRepository.ContarArtigosPorCategoriaAsync(id);
            var dto = new CategoriaDto(categoria.Id, categoria.Nome, categoria.Descricao, categoria.Ativo, totalArtigos);

            return Result<CategoriaDto>.Ok(dto);
        }

        public async Task<Result<CategoriaDto>> CriarAsync(CriarCategoriaDto dto)
        {
            // Validar nome duplicado
            if (await _categoriaRepository.NomeJaExisteAsync(dto.Nome))
                return Result<CategoriaDto>.Falhou("Já existe uma categoria com este nome");

            var categoria = new Categoria
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            await _categoriaRepository.AdicionarAsync(categoria);
            await _categoriaRepository.SaveChangesAsync();

            var categoriaDto = new CategoriaDto(categoria.Id, categoria.Nome, categoria.Descricao, categoria.Ativo, 0);
            return Result<CategoriaDto>.Ok(categoriaDto);
        }

        public async Task<Result<CategoriaDto>> AtualizarAsync(int id, AtualizarCategoriaDto dto)
        {
            var categoria = await _categoriaRepository.ObterPorIdAsync(id);

            if (categoria == null)
                return Result<CategoriaDto>.Falhou("Categoria não encontrada");

            // Validar nome duplicado (ignorar próprio ID)
            if (await _categoriaRepository.NomeJaExisteAsync(dto.Nome, id))
                return Result<CategoriaDto>.Falhou("Já existe uma categoria com este nome");

            categoria.Nome = dto.Nome;
            categoria.Descricao = dto.Descricao;

            await _categoriaRepository.AtualizarAsync(categoria);
            await _categoriaRepository.SaveChangesAsync();

            var totalArtigos = await _categoriaRepository.ContarArtigosPorCategoriaAsync(id);
            var categoriaDto = new CategoriaDto(categoria.Id, categoria.Nome, categoria.Descricao, categoria.Ativo, totalArtigos);

            return Result<CategoriaDto>.Ok(categoriaDto);
        }

        public async Task<Result<bool>> AlterarEstadoAsync(int id, bool ativo)
        {
            var categoria = await _categoriaRepository.ObterPorIdAsync(id);

            if (categoria == null)
                return Result<bool>.Falhou("Categoria não encontrada");

            categoria.Ativo = ativo;
            await _categoriaRepository.AtualizarAsync(categoria);
            await _categoriaRepository.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> ApagarAsync(int id)
        {
            var categoria = await _categoriaRepository.ObterPorIdAsync(id);

            if (categoria == null)
                return Result<bool>.Falhou("Categoria não encontrada");

            // Verificar se tem artigos
            var totalArtigos = await _categoriaRepository.ContarArtigosPorCategoriaAsync(id);
            if (totalArtigos > 0)
                return Result<bool>.Falhou($"Não é possível apagar. Categoria tem {totalArtigos} artigo(s) associado(s)");

            // Aqui podes implementar soft delete ou hard delete
            // Por agora, vou fazer soft delete (desativar)
            categoria.Ativo = false;
            await _categoriaRepository.AtualizarAsync(categoria);
            await _categoriaRepository.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }
    }
}