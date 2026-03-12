using Microsoft.AspNetCore.Mvc;

namespace SistemaPDI.Web.Models
{
    public class CriarListaViewModel
    {
        public string? Observacoes { get; set; }
        public List<LinhaListaViewModel> Linhas { get; set; } = new();
    }

    public class LinhaListaViewModel
    {
        public int ArtigoId { get; set; }
        public int QuantidadeRequisitada { get; set; }
    }

    public class SubmeterOrcamentoViewModel
    {
        public IFormFile? OrcamentoPdf { get; set; }
        public decimal ValorOrcamento { get; set; }
        public string? Observacoes { get; set; }
    }

    public class AprovarPreencherViewModel
    {
        public int FornecedorId { get; set; }
        public DateTime? DataEntregaPrevista { get; set; }
        public string? ObservacoesInternas { get; set; }
        public List<PreencherLinhaViewModel> Linhas { get; set; } = new();
    }

    public class PreencherLinhaViewModel
    {
        public int LinhaId { get; set; }
        public int QuantidadeAprovada { get; set; }
        public decimal PrecoUnitario { get; set; }
        public string? NumeroLote { get; set; }
        public DateTime? DataValidade { get; set; }
    }

    public class RegistarRececaoViewModel
    {
        public int EncomendaId { get; set; }
        public string? Observacoes { get; set; }
        public List<RececaoLinhaViewModel> Linhas { get; set; } = new();
    }

    public class RececaoLinhaViewModel
    {
        public int LinhaId { get; set; }
        public int QuantidadeRecebida { get; set; }
        public string NumeroLote { get; set; } = string.Empty;
        public DateTime DataValidade { get; set; }
        public int LocalizacaoId { get; set; }
        public string? Observacoes { get; set; }
    }
}