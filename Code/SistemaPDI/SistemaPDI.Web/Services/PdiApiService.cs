using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Domain.Entities;
using SistemaPDI.Web.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SistemaPDI.Web.Services
{
    public class PdiApiService : IPdiApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public PdiApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public void ConfigurarToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // ── Helper: le mensagem de erro do body HTTP ─────────────────────────
        private async Task<string> LerErroAsync(HttpResponseMessage response)
        {
            // Tratar códigos de erro específicos
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized: // 401
                    return "Sessão expirada ou inválida. Faça login novamente.";
                
                case System.Net.HttpStatusCode.Forbidden: // 403
                    return "Não tens permissão para realizar esta operação.";
                
                case System.Net.HttpStatusCode.NotFound: // 404
                    return "O recurso solicitado não foi encontrado.";
            }

            // Tentar ler mensagem do body da resposta
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);

                if (doc.RootElement.TryGetProperty("message", out var msg)) return msg.GetString()!;
                if (doc.RootElement.TryGetProperty("erro", out var erro)) return erro.GetString()!;
                if (doc.RootElement.TryGetProperty("errors", out var errors)) return "Dados inválidos ou rota inexistente.";
            }
            catch { }

            return $"Erro no servidor (Código: {(int)response.StatusCode}).";
        }

        // ── Helper: deserializa o body ────────────────────────────────────────
        private async Task<T?> DeserializarAsync<T>(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }
        // ── Helper: serializa objeto para StringContent JSON ─────────────────
        private StringContent Serializar<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        #region Auth
        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var content = Serializar(dto);
            var response = await _httpClient.PostAsync("api/auth/login", content);
            if (!response.IsSuccessStatusCode) return null;
            return await DeserializarAsync<LoginResponseDto>(response);
        }
        #endregion

        #region Utilizadores
        public async Task<ApiResult<List<UtilizadorDto>>> ObterUtilizadoresAsync()
        {
            var response = await _httpClient.GetAsync("api/utilizadores");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<UtilizadorDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<UtilizadorDto>>(response) ?? new();
            return ApiResult<List<UtilizadorDto>>.Ok(dados);
        }

        public async Task<ApiResult<UtilizadorDto>> ObterUtilizadorPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/utilizadores/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<UtilizadorDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<UtilizadorDto>(response);
            return ApiResult<UtilizadorDto>.Ok(dados!);
        }

        public async Task<ApiResult<UtilizadorDto>> CriarUtilizadorAsync(RegistarUtilizadorDto dto)
        {
            var response = await _httpClient.PostAsync("api/utilizadores", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<UtilizadorDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<UtilizadorDto>(response);
            return ApiResult<UtilizadorDto>.Ok(dados!);
        }

        public async Task<ApiResult<UtilizadorDto>> AtualizarUtilizadorAsync(int id, AtualizarUtilizadorDto dto)
        {
            var response = await _httpClient.PutAsync($"api/utilizadores/{id}", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<UtilizadorDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<UtilizadorDto>(response);
            return ApiResult<UtilizadorDto>.Ok(dados!);
        }

        public async Task<ApiResult> DesativarUtilizadorAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/utilizadores/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult> AtivarUtilizadorAsync(int id)
        {
            var response = await _httpClient.PatchAsync($"api/utilizadores/{id}/ativar", null);
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult> ResetPasswordAsync(int id, string novaPassword)
        {
            var dto = new { NovaPassword = novaPassword };
            var response = await _httpClient.PatchAsync($"api/utilizadores/{id}/reset-password", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }
        #endregion

        #region Artigos
        public async Task<ApiResult<List<ArtigoDto>>> ObterArtigosAsync()
        {
            var response = await _httpClient.GetAsync("api/artigos");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<ArtigoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<ArtigoDto>>(response) ?? new();
            return ApiResult<List<ArtigoDto>>.Ok(dados);
        }

        public async Task<ApiResult<ArtigoDto>> ObterArtigoPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/artigos/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<ArtigoDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<ArtigoDto>(response);
            return ApiResult<ArtigoDto>.Ok(dados!);
        }

        public async Task<ApiResult<List<ArtigoDto>>> ObterArtigosComStockBaixoAsync()
        {
            var response = await _httpClient.GetAsync("api/artigos/stock-baixo");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<ArtigoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<ArtigoDto>>(response) ?? new();
            return ApiResult<List<ArtigoDto>>.Ok(dados);
        }

        public async Task<ApiResult<List<ArtigoDto>>> ObterArtigosComStockCriticoAsync()
        {
            var response = await _httpClient.GetAsync("api/artigos/stock-critico");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<ArtigoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<ArtigoDto>>(response) ?? new();
            return ApiResult<List<ArtigoDto>>.Ok(dados);
        }

        public async Task<ApiResult<ArtigoDto>> CriarArtigoAsync(CriarArtigoDto dto, string? urlImagem)
        {
            // Se urlImagem foi fornecida, atualizar o DTO
            var dtoFinal = urlImagem != null ? dto with { UrlImagem = urlImagem } : dto;
            
            var response = await _httpClient.PostAsync("api/artigos", Serializar(dtoFinal));
            if (!response.IsSuccessStatusCode)
                return ApiResult<ArtigoDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<ArtigoDto>(response);
            return ApiResult<ArtigoDto>.Ok(dados!);
        }

        public async Task<ApiResult<ArtigoDto>> AtualizarArtigoAsync(int id, AtualizarArtigoDto dto)
        {
            var response = await _httpClient.PutAsync($"api/artigos/{id}", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<ArtigoDto>.Falhou(await LerErroAsync(response));

            // Verificar se a resposta tem conteúdo antes de deserializar
            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return ApiResult<ArtigoDto>.Ok(null!);
            }

            var dados = JsonSerializer.Deserialize<ArtigoDto>(body, _jsonOptions);
            return ApiResult<ArtigoDto>.Ok(dados!);
        }

        public async Task<ApiResult> AlternarEstadoArtigoAsync(int id)
        {
            var response = await _httpClient.PatchAsync($"api/artigos/{id}/toggle-ativo", null);
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult> RemoverArtigoAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/artigos/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult<List<ArtigoDto>>> ObterArtigosDesativadosAsync()
        {
            var response = await _httpClient.GetAsync("api/artigos/desativados");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<ArtigoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<ArtigoDto>>(response) ?? new();
            return ApiResult<List<ArtigoDto>>.Ok(dados);
        }
        #endregion

        #region Categorias
        public async Task<ApiResult<List<CategoriaDto>>> ObterCategoriasAsync(bool incluirInativos = false)
        {
            var url = incluirInativos ? "api/categorias?incluirInativos=true" : "api/categorias";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<CategoriaDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<CategoriaDto>>(response) ?? new();
            return ApiResult<List<CategoriaDto>>.Ok(dados);
        }

        public async Task<ApiResult<List<CategoriaDto>>> ObterCategoriasAtivasAsync()
        {
            var response = await _httpClient.GetAsync("api/categorias/ativas");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<CategoriaDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<CategoriaDto>>(response) ?? new();
            return ApiResult<List<CategoriaDto>>.Ok(dados);
        }

        public async Task<ApiResult<CategoriaDto>> ObterCategoriaPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/categorias/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<CategoriaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<CategoriaDto>(response);
            return ApiResult<CategoriaDto>.Ok(dados!);
        }

        public async Task<ApiResult<CategoriaDto>> CriarCategoriaAsync(CriarCategoriaDto dto)
        {
            var response = await _httpClient.PostAsync("api/categorias", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<CategoriaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<CategoriaDto>(response);
            return ApiResult<CategoriaDto>.Ok(dados!);
        }

        public async Task<ApiResult<CategoriaDto>> AtualizarCategoriaAsync(int id, AtualizarCategoriaDto dto)
        {
            var response = await _httpClient.PutAsync($"api/categorias/{id}", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<CategoriaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<CategoriaDto>(response);
            return ApiResult<CategoriaDto>.Ok(dados!);
        }

        public async Task<ApiResult> AlternarEstadoCategoriaAsync(int id)
        {
            var response = await _httpClient.PatchAsync($"api/categorias/{id}/toggle-ativo", null);
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult> RemoverCategoriaAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/categorias/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }
        #endregion

        #region Fornecedores
        // ══════════════════════════════════════════════════════════════════════
        // FORNECEDORES
        // ══════════════════════════════════════════════════════════════════════

        public async Task<ApiResult<List<FornecedorDto>>> ObterFornecedoresAsync(bool incluirInativos = false)
        {
            var url = incluirInativos ? "api/fornecedores?incluirInativos=true" : "api/fornecedores";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<FornecedorDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<FornecedorDto>>(response) ?? new();
            return ApiResult<List<FornecedorDto>>.Ok(dados);
        }

        public async Task<ApiResult<List<FornecedorDropdownDto>>> ObterFornecedoresDropdownAsync()
        {
            var response = await _httpClient.GetAsync("api/fornecedores/dropdown");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<FornecedorDropdownDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<FornecedorDropdownDto>>(response) ?? new();
            return ApiResult<List<FornecedorDropdownDto>>.Ok(dados);
        }

        public async Task<ApiResult<List<FornecedorDropdownDto>>> ObterFornecedoresPreferenciaisAsync()
        {
            var response = await _httpClient.GetAsync("api/fornecedores/preferenciais");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<FornecedorDropdownDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<FornecedorDropdownDto>>(response) ?? new();
            return ApiResult<List<FornecedorDropdownDto>>.Ok(dados);
        }

        public async Task<ApiResult<FornecedorDto>> ObterFornecedorPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/fornecedores/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<FornecedorDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<FornecedorDto>(response);
            return ApiResult<FornecedorDto>.Ok(dados!);
        }

        public async Task<ApiResult<FornecedorDto>> CriarFornecedorAsync(CriarFornecedorDto dto)
        {
            var response = await _httpClient.PostAsync("api/fornecedores", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<FornecedorDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<FornecedorDto>(response);
            return ApiResult<FornecedorDto>.Ok(dados!);
        }

        public async Task<ApiResult<FornecedorDto>> AtualizarFornecedorAsync(int id, AtualizarFornecedorDto dto)
        {
            var response = await _httpClient.PutAsync($"api/fornecedores/{id}", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<FornecedorDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<FornecedorDto>(response);
            return ApiResult<FornecedorDto>.Ok(dados!);
        }

        public async Task<ApiResult> ToggleAtivoFornecedorAsync(int id)
        {
            var response = await _httpClient.PatchAsync($"api/fornecedores/{id}/toggle-ativo", null);
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult> TogglePreferencialFornecedorAsync(int id)
        {
            var response = await _httpClient.PatchAsync($"api/fornecedores/{id}/toggle-preferencial", null);
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult> ApagarFornecedorAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/fornecedores/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }
        #endregion

        #region Lotes
        // ══════════════════════════════════════════════════════════════════════════════
        // LOTES
        // ══════════════════════════════════════════════════════════════════════════════
        public async Task<ApiResult<List<LoteDto>>> ObterLotesAsync()
        {
            var response = await _httpClient.GetAsync("api/lotes");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<LoteDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<LoteDto>>(response) ?? new();
            return ApiResult<List<LoteDto>>.Ok(dados);
        }

        public async Task<ApiResult<LoteDto>> ObterLotePorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/lotes/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<LoteDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<LoteDto>(response);
            return ApiResult<LoteDto>.Ok(dados!);
        }

        public async Task<ApiResult<List<LoteDto>>> ObterLotesPorArtigoAsync(int artigoId)
        {
            var response = await _httpClient.GetAsync($"api/lotes/artigo/{artigoId}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<LoteDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<LoteDto>>(response) ?? new();
            return ApiResult<List<LoteDto>>.Ok(dados);
        }

        public async Task<ApiResult<LoteDto>> CriarLoteAsync(CriarLoteDto dto)
        {
            var response = await _httpClient.PostAsync("api/lotes", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<LoteDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<LoteDto>(response);
            return ApiResult<LoteDto>.Ok(dados!);
        }

        public async Task<ApiResult<LoteDto>> AtualizarLoteAsync(int id, AtualizarLoteDto dto)
        {
            var response = await _httpClient.PutAsync($"api/lotes/{id}", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<LoteDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<LoteDto>(response);
            return ApiResult<LoteDto>.Ok(dados!);
        }

        public async Task<ApiResult> DesativarLoteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/lotes/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult<ResultadoReservaDto>> ReservarStockAsync(ReservaStockDto dto)
        {
            var response = await _httpClient.PostAsync("api/lotes/reservar", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<ResultadoReservaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<ResultadoReservaDto>(response);
            return ApiResult<ResultadoReservaDto>.Ok(dados!);
        }

        public async Task<ApiResult> LibertarReservaAsync(LibertarReservaDto dto)
        {
            var response = await _httpClient.PostAsync("api/lotes/libertar-reserva", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult> ConfirmarSaidaLoteAsync(int loteId, int quantidade)
        {
            var response = await _httpClient.PostAsync(
                $"api/lotes/{loteId}/confirmar-saida",
                Serializar(new { Quantidade = quantidade }));

            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult<int>> ObterStockDisponivelAsync(int artigoId)
        {
            var response = await _httpClient.GetAsync($"api/lotes/stock-disponivel/{artigoId}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<int>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<StockDisponivelResponse>(response);
            return ApiResult<int>.Ok(dados?.StockDisponivel ?? 0);
        }

        private record StockDisponivelResponse(int ArtigoId, int StockDisponivel);
        #endregion

        #region Localizações
        // ══════════════════════════════════════════════════════════════════════════════
        // LOCALIZAÇÕES
        // ══════════════════════════════════════════════════════════════════════════════

        public async Task<ApiResult<List<LocalizacaoDto>>> ObterLocalizacoesAsync(bool incluirInativos = false)
        {
            var url = incluirInativos ? "api/localizacoes?incluirInativos=true" : "api/localizacoes";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<LocalizacaoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<LocalizacaoDto>>(response) ?? new();
            return ApiResult<List<LocalizacaoDto>>.Ok(dados);
        }

        public async Task<ApiResult<List<LocalizacaoDto>>> ObterLocalizacoesAtivasAsync()
        {
            var response = await _httpClient.GetAsync("api/localizacoes/ativas");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<LocalizacaoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<LocalizacaoDto>>(response) ?? new();
            return ApiResult<List<LocalizacaoDto>>.Ok(dados);
        }

        public async Task<ApiResult<LocalizacaoDto>> ObterLocalizacaoPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/localizacoes/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<LocalizacaoDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<LocalizacaoDto>(response);
            return ApiResult<LocalizacaoDto>.Ok(dados!);
        }

        public async Task<ApiResult<List<LocalizacaoDropdownDto>>> ObterLocalizacoesDropdownAsync()
        {
            var response = await _httpClient.GetAsync("api/localizacoes/dropdown");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<LocalizacaoDropdownDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<LocalizacaoDropdownDto>>(response) ?? new();
            return ApiResult<List<LocalizacaoDropdownDto>>.Ok(dados);
        }

        public async Task<ApiResult<LocalizacaoDto>> CriarLocalizacaoAsync(CriarLocalizacaoDto dto)
        {
            var response = await _httpClient.PostAsync("api/localizacoes", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<LocalizacaoDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<LocalizacaoDto>(response);
            return ApiResult<LocalizacaoDto>.Ok(dados!);
        }

        public async Task<ApiResult<LocalizacaoDto>> AtualizarLocalizacaoAsync(int id, AtualizarLocalizacaoDto dto)
        {
            var response = await _httpClient.PutAsync($"api/localizacoes/{id}", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<LocalizacaoDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<LocalizacaoDto>(response);
            return ApiResult<LocalizacaoDto>.Ok(dados!);
        }

        public async Task<ApiResult> ToggleAtivoLocalizacaoAsync(int id)
        {
            var response = await _httpClient.PatchAsync($"api/localizacoes/{id}/toggle-ativo", null);
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult> ApagarLocalizacaoAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/localizacoes/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }
        #endregion

        #region Encomendas
        // ══════════════════════════════════════════════════════════════════════
        // ENCOMENDAS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<ApiResult<List<EncomendaDto>>> ObterEncomendasAsync(bool incluirInativos = false)
        {
            var url = incluirInativos ? "api/encomendas?incluirInativos=true" : "api/encomendas";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<EncomendaDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<EncomendaDto>>(response) ?? new();
            return ApiResult<List<EncomendaDto>>.Ok(dados);
        }

        public async Task<ApiResult<EncomendaDto>> ObterEncomendaPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/encomendas/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<EncomendaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<EncomendaDto>(response);
            return ApiResult<EncomendaDto>.Ok(dados!);
        }

        public async Task<ApiResult<List<EncomendaDto>>> ObterEncomendaPorEstadoAsync(string estado)
        {
            var response = await _httpClient.GetAsync($"api/encomendas/estado/{estado}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<EncomendaDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<EncomendaDto>>(response) ?? new();
            return ApiResult<List<EncomendaDto>>.Ok(dados);
        }

        public async Task<ApiResult<List<EncomendaDto>>> ObterMinhasEncomendasAsync()
        {
            var response = await _httpClient.GetAsync("api/encomendas/minhas");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<EncomendaDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<EncomendaDto>>(response) ?? new();
            return ApiResult<List<EncomendaDto>>.Ok(dados);
        }

        public async Task<ApiResult<List<EncomendaDto>>> ObterPendentesAprovacaoAsync()
        {
            var response = await _httpClient.GetAsync("api/encomendas/pendentes-aprovacao");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<EncomendaDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<EncomendaDto>>(response) ?? new();
            return ApiResult<List<EncomendaDto>>.Ok(dados);
        }

        public async Task<ApiResult<List<EncomendaDto>>> ObterEncomendasEnviadasAsync()
        {
            var response = await _httpClient.GetAsync("api/encomendas/enviadas");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<EncomendaDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<EncomendaDto>>(response) ?? new();
            return ApiResult<List<EncomendaDto>>.Ok(dados);
        }

        public async Task<ApiResult<EncomendaDto>> CriarListaAsync(CriarListaDto dto)
        {
            var response = await _httpClient.PostAsync("api/encomendas/lista", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<EncomendaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<EncomendaDto>(response);
            return ApiResult<EncomendaDto>.Ok(dados!);
        }

        public async Task<ApiResult<EncomendaDto>> AtualizarListaAsync(int id, CriarListaDto dto)
        {
            var response = await _httpClient.PutAsync($"api/encomendas/{id}/lista", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<EncomendaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<EncomendaDto>(response);
            return ApiResult<EncomendaDto>.Ok(dados!);
        }

        public async Task<ApiResult<byte[]>> GerarPdfAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/encomendas/{id}/gerar-pdf");
            if (!response.IsSuccessStatusCode)
                return ApiResult<byte[]>.Falhou(await LerErroAsync(response));

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return ApiResult<byte[]>.Ok(bytes);
        }

        public async Task<ApiResult<EncomendaDto>> MarcarComoRascunhoAsync(int id)
        {
            var response = await _httpClient.PostAsync($"api/encomendas/{id}/marcar-rascunho", null);
            if (!response.IsSuccessStatusCode)
                return ApiResult<EncomendaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<EncomendaDto>(response);
            return ApiResult<EncomendaDto>.Ok(dados!);
        }
        public async Task<ApiResult<EncomendaDto>> SubmeterOrcamentoAsync(int id, SubmeterOrcamentoDto dto)
        {
            var response = await _httpClient.PostAsync(
                $"api/encomendas/{id}/submeter-orcamento",
                Serializar(dto)
            );

            if (!response.IsSuccessStatusCode)
                return ApiResult<EncomendaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<EncomendaDto>(response);
            return ApiResult<EncomendaDto>.Ok(dados!);
        }

        public async Task<ApiResult<EncomendaDto>> RejeitarEncomendaAsync(int id, string motivo)
        {
            var response = await _httpClient.PostAsync($"api/encomendas/{id}/rejeitar", Serializar(new { Motivo = motivo }));
            if (!response.IsSuccessStatusCode)
                return ApiResult<EncomendaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<EncomendaDto>(response);
            return ApiResult<EncomendaDto>.Ok(dados!);
        }

        public async Task<ApiResult<EncomendaDto>> AprovarEPreencherAsync(int id, AprovarEPreencherDto dto)
        {
            var response = await _httpClient.PostAsync($"api/encomendas/{id}/aprovar-preencher", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<EncomendaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<EncomendaDto>(response);
            return ApiResult<EncomendaDto>.Ok(dados!);
        }

        public async Task<ApiResult<EncomendaDto>> ConfirmarEEnviarAsync(int id)
        {
            var response = await _httpClient.PostAsync($"api/encomendas/{id}/confirmar-enviar", null);
            if (!response.IsSuccessStatusCode)
                return ApiResult<EncomendaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<EncomendaDto>(response);
            return ApiResult<EncomendaDto>.Ok(dados!);
        }

        public async Task<ApiResult<EncomendaDto>> RegistarRecepcaoAsync(RegistarRecepcaoDto dto)
        {
            var response = await _httpClient.PostAsync("api/encomendas/registar-recepcao", Serializar(dto));
            if (!response.IsSuccessStatusCode)
                return ApiResult<EncomendaDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<EncomendaDto>(response);
            return ApiResult<EncomendaDto>.Ok(dados!);
        }

        public async Task<ApiResult> CancelarEncomendaAsync(int id, string motivo)
        {
            var response = await _httpClient.PostAsync($"api/encomendas/{id}/cancelar", Serializar(new { Motivo = motivo }));
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }

        public async Task<ApiResult> ToggleAtivoEncomendaAsync(int id)
        {
            var response = await _httpClient.PatchAsync($"api/encomendas/{id}/toggle-ativo", null);
            if (!response.IsSuccessStatusCode)
                return ApiResult.Falhou(await LerErroAsync(response));

            return ApiResult.Ok();
        }
        #endregion

        #region HistoricoPrecos
        // ══════════════════════════════════════════════════════════════════════
        // HISTÓRICO DE PREÇOS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPrecosAsync()
        {
            var response = await _httpClient.GetAsync($"/api/historicosprecos");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<HistoricoPrecoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<HistoricoPrecoDto>>(response) ?? new();
            return ApiResult<List<HistoricoPrecoDto>>.Ok(dados);
        }

        public async Task<ApiResult<HistoricoPrecoDto>> ObterHistoricoPrecoPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/historicosprecos/{id}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<HistoricoPrecoDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<HistoricoPrecoDto>(response);
            return ApiResult<HistoricoPrecoDto>.Ok(dados!);
        }

        public async Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPorArtigoAsync(int artigoId)
        {
            var response = await _httpClient.GetAsync($"/api/historicosprecos/artigo/{artigoId}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<HistoricoPrecoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<HistoricoPrecoDto>>(response);
            return ApiResult<List<HistoricoPrecoDto>>.Ok(dados!);
        }

        public async Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPorFornecedorAsync(int fornecedorId)
        {
            var response = await _httpClient.GetAsync($"/api/historicosprecos/fornecedor/{fornecedorId}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<HistoricoPrecoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<HistoricoPrecoDto>>(response);
            return ApiResult<List<HistoricoPrecoDto>>.Ok(dados!);
        }

        public async Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPorEncomendaAsync(int encomendaId)
        {
            var response = await _httpClient.GetAsync($"/api/historicosprecos/encomenda/{encomendaId}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<HistoricoPrecoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<HistoricoPrecoDto>>(response);
            return ApiResult<List<HistoricoPrecoDto>>.Ok(dados!);
        }

        public async Task<ApiResult<List<HistoricoPrecoDto>>> ObterHistoricoPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            var url = $"/api/historicosprecos/periodo?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<HistoricoPrecoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<HistoricoPrecoDto>>(response);
            return ApiResult<List<HistoricoPrecoDto>>.Ok(dados!);
        }

        public async Task<ApiResult<List<EvolucaoPrecoDto>>> ObterEvolucaoPrecosAsync(int artigoId)
        {
            var response = await _httpClient.GetAsync($"/api/historicosprecos/evolucao/{artigoId}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<EvolucaoPrecoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<EvolucaoPrecoDto>>(response);
            return ApiResult<List<EvolucaoPrecoDto>>.Ok(dados!);
        }

        public async Task<ApiResult<ComparacaoFornecedorDto>> CompararFornecedoresAsync(int artigoId)
        {
            var response = await _httpClient.GetAsync($"/api/historicosprecos/comparar-fornecedores/{artigoId}");
            if (!response.IsSuccessStatusCode)
                return ApiResult<ComparacaoFornecedorDto>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<ComparacaoFornecedorDto>(response);
            return ApiResult<ComparacaoFornecedorDto>.Ok(dados!);
        }

        public async Task<ApiResult<List<SugestaoPrecoDto>>> ObterSugestoesPrecosAsync(List<int> artigosIds, int? fornecedorId = null)
        {
            var request = new { ArtigosIds = artigosIds, FornecedorId = fornecedorId };
            var content = Serializar(request);
            var response = await _httpClient.PostAsync("/api/historicosprecos/sugestoes", content);
            if (!response.IsSuccessStatusCode)
                return ApiResult<List<SugestaoPrecoDto>>.Falhou(await LerErroAsync(response));

            var dados = await DeserializarAsync<List<SugestaoPrecoDto>>(response);
            return ApiResult<List<SugestaoPrecoDto>>.Ok(dados!);
        }
        #endregion
    }
}