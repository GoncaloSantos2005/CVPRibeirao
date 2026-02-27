using Microsoft.AspNetCore.Mvc;

namespace SistemaPDI.Web.Models
{
    public class ApiResult<T>
    {
        public bool Sucesso { get; set; }
        public T? Dados { get; set; }
        public string? Erro { get; set; }

        public static ApiResult<T> Ok(T dados)
            => new() { Sucesso = true, Dados = dados };

        public static ApiResult<T> Falhou(string erro)
            => new() { Sucesso = false, Erro = erro };
    }

    public class ApiResult
    {
        public bool Sucesso { get; set; }
        public string? Erro { get; set; }

        public static ApiResult Ok()
            => new() { Sucesso = true };

        public static ApiResult Falhou(string erro)
            => new() { Sucesso = false, Erro = erro };
    }
}
