using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace SistemaPDI.Web.Services
{
    public class ImagemService : IImagemService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly long _tamanhoMaximo = 5 * 1024 * 1024; // 5MB
        private readonly string[] _extensoesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ImagemService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> GuardarImagemAsync(IFormFile ficheiro, string pasta = "artigos")
        {
            if (!ValidarImagem(ficheiro))
                throw new InvalidOperationException("Ficheiro inválido");

            // Nome único
            var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
            var nomeUnico = $"{Guid.NewGuid()}{extensao}";

            // Caminho completo
            var pastaDestino = Path.Combine(_environment.WebRootPath, "uploads", pasta);

            if (!Directory.Exists(pastaDestino))
                Directory.CreateDirectory(pastaDestino);

            var caminhoCompleto = Path.Combine(pastaDestino, nomeUnico);

            // Guardar
            using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
            {
                await ficheiro.CopyToAsync(stream);
            }

            // Retornar URL relativo
            return $"/uploads/{pasta}/{nomeUnico}";
        }

        public Task<bool> ApagarImagemAsync(string caminho)
        {
            if (string.IsNullOrEmpty(caminho))
                return Task.FromResult(false);

            var caminhoCompleto = Path.Combine(_environment.WebRootPath, caminho.TrimStart('/'));

            if (File.Exists(caminhoCompleto))
            {
                File.Delete(caminhoCompleto);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public bool ValidarImagem(IFormFile ficheiro)
        {
            if (ficheiro == null || ficheiro.Length == 0)
                return false;

            if (ficheiro.Length > _tamanhoMaximo)
                return false;

            var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
            if (!_extensoesPermitidas.Contains(extensao))
                return false;

            return true;
        }
    }
}