using Microsoft.AspNetCore.Http;

namespace SistemaPDI.Web.Services
{
    public interface IImagemService
    {
        Task<string> GuardarImagemAsync(IFormFile ficheiro, string pasta = "artigos");
        Task<bool> ApagarImagemAsync(string caminho);
        bool ValidarImagem(IFormFile ficheiro);
    }
}