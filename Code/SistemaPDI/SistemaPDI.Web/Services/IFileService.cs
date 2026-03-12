namespace SistemaPDI.Web.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Guarda um ficheiro no servidor
        /// </summary>
        /// <param name="ficheiro">Ficheiro a guardar</param>
        /// <param name="pasta">Pasta destino (ex: "orcamentos", "artigos")</param>
        /// <param name="prefixo">Prefixo opcional para o nome do ficheiro</param>
        /// <returns>Caminho relativo do ficheiro guardado</returns>
        Task<string> GuardarFicheiroAsync(IFormFile ficheiro, string pasta, string? prefixo = null);

        /// <summary>
        /// Apaga um ficheiro do servidor
        /// </summary>
        /// <param name="caminhoRelativo">Caminho relativo do ficheiro</param>
        /// <returns>True se apagou com sucesso</returns>
        Task<bool> ApagarFicheiroAsync(string caminhoRelativo);

        /// <summary>
        /// Valida um ficheiro PDF
        /// </summary>
        bool ValidarPdf(IFormFile ficheiro, long tamanhoMaximoMb = 10);

        /// <summary>
        /// Valida uma imagem
        /// </summary>
        bool ValidarImagem(IFormFile ficheiro, long tamanhoMaximoMb = 5);

        /// <summary>
        /// Verifica se um ficheiro existe
        /// </summary>
        bool FicheiroExiste(string caminhoRelativo);

        /// <summary>
        /// Obtém o caminho físico completo de um ficheiro
        /// </summary>
        string ObterCaminhoFisico(string caminhoRelativo);
    }
}