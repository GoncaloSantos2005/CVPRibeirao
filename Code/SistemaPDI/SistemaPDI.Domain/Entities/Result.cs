using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Domain.Entities
{
    public class Result<T>
    {
        public bool Sucesso { get; }
        public T? Dados { get; }
        public string? Erro { get; }

        private Result(bool sucesso, T? dados, string? erro)
        {
            Sucesso = sucesso;
            Dados = dados;
            Erro = erro;
        }
        public static Result<T> Ok(T dados) => new(true, dados, null);
        public static Result<T> Falhou(string erro) => new(false, default, erro);
    }

    public class Result
    {
        public bool Sucesso { get; }
        public string? Erro { get; }

        private Result(bool sucesso, string? erro)
        {
            Sucesso = sucesso;
            Erro = erro;
        }

        public static Result Ok() => new(true, null);
        public static Result Falhou(string erro) => new(false, erro);
    }
}
