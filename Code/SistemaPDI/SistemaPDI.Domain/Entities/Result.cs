using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaPDI.Domain.Entities
{
    /// <summary>
    /// Representa o resultado de uma operação com dados tipados.
    /// </summary>
    /// <typeparam name="T">Tipo dos dados retornados.</typeparam>
    public class Result<T>
    {
        /// <summary>Indica se a operação foi bem-sucedida.</summary>
        public bool Sucesso { get; }

        /// <summary>Dados retornados (se sucesso).</summary>
        public T? Dados { get; }

        /// <summary>Mensagem de erro (se falhou).</summary>
        public string? Erro { get; }

        private Result(bool sucesso, T? dados, string? erro)
        {
            Sucesso = sucesso;
            Dados = dados;
            Erro = erro;
        }

        /// <summary>Cria um resultado de sucesso com dados.</summary>
        public static Result<T> Ok(T dados) => new(true, dados, null);

        /// <summary>Cria um resultado de falha com mensagem de erro.</summary>
        public static Result<T> Falhou(string erro) => new(false, default, erro);
    }

    /// <summary>
    /// Representa o resultado de uma operação sem dados de retorno.
    /// </summary>
    public class Result
    {
        /// <summary>Indica se a operação foi bem-sucedida.</summary>
        public bool Sucesso { get; }

        /// <summary>Mensagem de erro (se falhou).</summary>
        public string? Erro { get; }

        private Result(bool sucesso, string? erro)
        {
            Sucesso = sucesso;
            Erro = erro;
        }

        /// <summary>Cria um resultado de sucesso.</summary>
        public static Result Ok() => new(true, null);

        /// <summary>Cria um resultado de falha com mensagem de erro.</summary>
        public static Result Falhou(string erro) => new(false, erro);
    }
}
