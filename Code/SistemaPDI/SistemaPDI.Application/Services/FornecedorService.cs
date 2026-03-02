using SistemaPDI.Application.Interfaces.IRepositories;
using SistemaPDI.Application.Interfaces.IServices;
using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;

namespace SistemaPDI.Application.Services
{
    public class FornecedorService : IFornecedorService
    {
        private readonly IFornecedorRepository _fornecedorRepository;

        public FornecedorService(IFornecedorRepository fornecedorRepository)
        {
            _fornecedorRepository = fornecedorRepository;
        }

        public async Task<Result<List<FornecedorDto>>> ObterTodosAsync(bool incluirInativos = false)
        {
            var fornecedores = await _fornecedorRepository.ObterTodosAsync(incluirInativos);

            var dtos = new List<FornecedorDto>();
            foreach (var f in fornecedores)
            {
                var totalEncomendas = await _fornecedorRepository.ContarEncomendasAsync(f.Id);
                dtos.Add(MapearParaDto(f, totalEncomendas));
            }

            return Result<List<FornecedorDto>>.Ok(dtos);
        }

        public async Task<Result<List<FornecedorDropdownDto>>> ObterAtivosParaDropdownAsync()
        {
            var fornecedores = await _fornecedorRepository.ObterAtivosAsync();

            var dtos = fornecedores
                .OrderByDescending(f => f.Preferencial) // Preferenciais primeiro
                .ThenBy(f => f.Nome)
                .Select(f => new FornecedorDropdownDto(
                    f.Id,
                    f.Nome,
                    f.Preferencial,
                    f.TempoEntrega
                ))
                .ToList();

            return Result<List<FornecedorDropdownDto>>.Ok(dtos);
        }

        public async Task<Result<List<FornecedorDropdownDto>>> ObterPreferenciaisParaDropdownAsync()
        {
            var fornecedores = await _fornecedorRepository.ObterPreferenciaisAsync();

            var dtos = fornecedores
                .OrderBy(f => f.Nome)
                .Select(f => new FornecedorDropdownDto(
                    f.Id,
                    f.Nome,
                    f.Preferencial,
                    f.TempoEntrega
                ))
                .ToList();

            return Result<List<FornecedorDropdownDto>>.Ok(dtos);
        }

        public async Task<Result<FornecedorDto>> ObterPorIdAsync(int id)
        {
            var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id);

            if (fornecedor == null)
                return Result<FornecedorDto>.Falhou("Fornecedor não encontrado");

            var totalEncomendas = await _fornecedorRepository.ContarEncomendasAsync(id);
            var dto = MapearParaDto(fornecedor, totalEncomendas);

            return Result<FornecedorDto>.Ok(dto);
        }

        public async Task<Result<FornecedorDto>> CriarAsync(CriarFornecedorDto dto)
        {
            // Validar NIF duplicado
            if (await _fornecedorRepository.NIFJaExisteAsync(dto.NIF))
                return Result<FornecedorDto>.Falhou("Já existe um fornecedor com este NIF");

            // Validar Nome duplicado
            if (await _fornecedorRepository.NomeJaExisteAsync(dto.Nome))
                return Result<FornecedorDto>.Falhou("Já existe um fornecedor com este nome");

            var fornecedor = new Fornecedor
            {
                Nome = dto.Nome.Trim(),
                NIF = dto.NIF.Trim(),
                Email = dto.Email?.Trim(),
                Telefone = dto.Telefone?.Trim(),
                PessoaContacto = dto.PessoaContacto?.Trim(),
                Morada = dto.Morada?.Trim(),
                CodigoPostal = dto.CodigoPostal?.Trim(),
                Localidade = dto.Localidade?.Trim(),
                TempoEntrega = dto.TempoEntrega > 0 ? dto.TempoEntrega : 3,
                Observacoes = dto.Observacoes?.Trim(),
                Ativo = true,
                Preferencial = false,
                CriadoEm = DateTime.UtcNow
            };

            await _fornecedorRepository.AdicionarAsync(fornecedor);
            await _fornecedorRepository.SaveChangesAsync();

            var fornecedorDto = MapearParaDto(fornecedor, 0);
            return Result<FornecedorDto>.Ok(fornecedorDto);
        }

        public async Task<Result<FornecedorDto>> AtualizarAsync(int id, AtualizarFornecedorDto dto)
        {
            var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id);

            if (fornecedor == null)
                return Result<FornecedorDto>.Falhou("Fornecedor não encontrado");

            // Validar NIF duplicado (ignorar próprio ID)
            if (await _fornecedorRepository.NIFJaExisteAsync(dto.NIF, id))
                return Result<FornecedorDto>.Falhou("Já existe um fornecedor com este NIF");

            // Validar Nome duplicado (ignorar próprio ID)
            if (await _fornecedorRepository.NomeJaExisteAsync(dto.Nome, id))
                return Result<FornecedorDto>.Falhou("Já existe um fornecedor com este nome");

            // Atualizar campos
            fornecedor.Nome = dto.Nome.Trim();
            fornecedor.NIF = dto.NIF.Trim();
            fornecedor.Email = dto.Email?.Trim();
            fornecedor.Telefone = dto.Telefone?.Trim();
            fornecedor.PessoaContacto = dto.PessoaContacto?.Trim();
            fornecedor.Morada = dto.Morada?.Trim();
            fornecedor.CodigoPostal = dto.CodigoPostal?.Trim();
            fornecedor.Localidade = dto.Localidade?.Trim();
            fornecedor.TempoEntrega = dto.TempoEntrega > 0 ? dto.TempoEntrega : 3;
            fornecedor.Observacoes = dto.Observacoes?.Trim();
            fornecedor.AtualizadoEm = DateTime.UtcNow;

            await _fornecedorRepository.AtualizarAsync(fornecedor);
            await _fornecedorRepository.SaveChangesAsync();

            var totalEncomendas = await _fornecedorRepository.ContarEncomendasAsync(id);
            var fornecedorDto = MapearParaDto(fornecedor, totalEncomendas);

            return Result<FornecedorDto>.Ok(fornecedorDto);
        }

        public async Task<Result<bool>> AlterarEstadoAsync(int id, bool ativo)
        {
            var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id);

            if (fornecedor == null)
                return Result<bool>.Falhou("Fornecedor não encontrado");

            fornecedor.Ativo = ativo;
            fornecedor.AtualizadoEm = DateTime.UtcNow;

            await _fornecedorRepository.AtualizarAsync(fornecedor);
            await _fornecedorRepository.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> TogglePreferencialAsync(int id)
        {
            var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id);

            if (fornecedor == null)
                return Result<bool>.Falhou("Fornecedor não encontrado");

            fornecedor.Preferencial = !fornecedor.Preferencial;
            fornecedor.AtualizadoEm = DateTime.UtcNow;

            await _fornecedorRepository.AtualizarAsync(fornecedor);
            await _fornecedorRepository.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> ApagarAsync(int id)
        {
            var fornecedor = await _fornecedorRepository.ObterPorIdAsync(id);

            if (fornecedor == null)
                return Result<bool>.Falhou("Fornecedor não encontrado");

            // Verificar se tem encomendas
            var totalEncomendas = await _fornecedorRepository.ContarEncomendasAsync(id);
            if (totalEncomendas > 0)
                return Result<bool>.Falhou($"Não é possível apagar. Fornecedor tem {totalEncomendas} encomenda(s) associada(s)");

            // Soft delete (desativar)
            fornecedor.Ativo = false;
            fornecedor.AtualizadoEm = DateTime.UtcNow;

            await _fornecedorRepository.AtualizarAsync(fornecedor);
            await _fornecedorRepository.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }

        // Helper para mapear
        private FornecedorDto MapearParaDto(Fornecedor f, int totalEncomendas)
        {
            return new FornecedorDto(
                f.Id,
                f.Nome,
                f.NIF,
                f.Email,
                f.Telefone,
                f.PessoaContacto,
                f.Morada,
                f.CodigoPostal,
                f.Localidade,
                f.TempoEntrega,
                f.Observacoes,
                f.Ativo,
                f.Preferencial,
                f.CriadoEm,
                totalEncomendas
            );
        }
    }
}