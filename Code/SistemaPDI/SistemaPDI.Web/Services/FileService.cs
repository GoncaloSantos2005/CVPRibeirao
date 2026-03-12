using Microsoft.AspNetCore.Mvc;

namespace SistemaPDI.Web.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        // Configurações
        private readonly string[] _extensoesPdf = { ".pdf" };
        private readonly string[] _extensoesImagem = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> GuardarFicheiroAsync(IFormFile ficheiro, string pasta, string? prefixo = null)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                throw new ArgumentException("Ficheiro inválido ou vazio");

            try
            {
                // Gerar nome único
                var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
                var nomeBase = string.IsNullOrEmpty(prefixo) 
                    ? Guid.NewGuid().ToString() 
                    : $"{prefixo}-{Guid.NewGuid()}";
                var nomeFicheiro = $"{nomeBase}{extensao}";

                // Criar pasta se não existir
                var pastaDestino = Path.Combine(_environment.WebRootPath, "uploads", pasta);
                if (!Directory.Exists(pastaDestino))
                {
                    Directory.CreateDirectory(pastaDestino);
                    _logger.LogInformation("Pasta criada: {Pasta}", pastaDestino);
                }

                // Caminho completo
                var caminhoCompleto = Path.Combine(pastaDestino, nomeFicheiro);

                // Guardar ficheiro
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await ficheiro.CopyToAsync(stream);
                }

                var caminhoRelativo = $"/uploads/{pasta}/{nomeFicheiro}";
                _logger.LogInformation("Ficheiro guardado: {Caminho}", caminhoRelativo);

                return caminhoRelativo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao guardar ficheiro na pasta {Pasta}", pasta);
                throw;
            }
        }

        public Task<bool> ApagarFicheiroAsync(string caminhoRelativo)
        {
            if (string.IsNullOrEmpty(caminhoRelativo))
                return Task.FromResult(false);

            try
            {
                var caminhoCompleto = ObterCaminhoFisico(caminhoRelativo);

                if (File.Exists(caminhoCompleto))
                {
                    File.Delete(caminhoCompleto);
                    _logger.LogInformation("Ficheiro apagado: {Caminho}", caminhoRelativo);
                    return Task.FromResult(true);
                }

                _logger.LogWarning("Ficheiro não encontrado para apagar: {Caminho}", caminhoRelativo);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao apagar ficheiro: {Caminho}", caminhoRelativo);
                return Task.FromResult(false);
            }
        }

        public bool ValidarPdf(IFormFile ficheiro, long tamanhoMaximoMb = 10)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                return false;

            var tamanhoMaximoBytes = tamanhoMaximoMb * 1024 * 1024;
            if (ficheiro.Length > tamanhoMaximoBytes)
                return false;

            var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
            if (!_extensoesPdf.Contains(extensao))
                return false;

            // Verificar content type
            if (ficheiro.ContentType != "application/pdf")
                return false;

            return true;
        }

        public bool ValidarImagem(IFormFile ficheiro, long tamanhoMaximoMb = 5)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                return false;

            var tamanhoMaximoBytes = tamanhoMaximoMb * 1024 * 1024;
            if (ficheiro.Length > tamanhoMaximoBytes)
                return false;

            var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
            if (!_extensoesImagem.Contains(extensao))
                return false;

            return true;
        }

        public bool FicheiroExiste(string caminhoRelativo)
        {
            if (string.IsNullOrEmpty(caminhoRelativo))
                return false;

            var caminhoCompleto = ObterCaminhoFisico(caminhoRelativo);
            return File.Exists(caminhoCompleto);
        }

        public string ObterCaminhoFisico(string caminhoRelativo)
        {
            if (string.IsNullOrEmpty(caminhoRelativo))
                return string.Empty;

            // Remover a barra inicial se existir
            var caminhoLimpo = caminhoRelativo.TrimStart('/');
            return Path.Combine(_environment.WebRootPath, caminhoLimpo);
        }
    }
}
