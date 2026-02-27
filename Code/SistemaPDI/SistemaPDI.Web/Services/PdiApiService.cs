using SistemaPDI.Contracts.DTOs;
using SistemaPDI.Web.Models;
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

        // ── Helper: serializa objeto para StringContent JSON ─────────────────
        private StringContent Serializar<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}