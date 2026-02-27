using FluentValidation;
using SistemaPDI.Contracts.DTOs;

namespace SistemaPDI.Application.Validations.Artigos
{
    public class AtualizarArtigoValidator : AbstractValidator<AtualizarArtigoDto>
    {
        public AtualizarArtigoValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome do artigo é obrigatório.")
                .MaximumLength(100).WithMessage("O nome não pode exceder os 100 caracteres.");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("O SKU/Código de Barras é obrigatório.")
                .Matches(@"^[A-Z0-9-]+$").WithMessage("O SKU só pode conter letras maiúsculas, números e hífens.");

            RuleFor(x => x.CategoriaId)
                .GreaterThan(0).WithMessage("É obrigatório selecionar uma CategoriaDtos.");

            // Limites de Alarme (Faz sentido atualizar aqui)
            RuleFor(x => x.StockMinimo)
                .GreaterThan(0).WithMessage("O Stock Mínimo tem de ser pelo menos 1.");

            RuleFor(x => x.StockCritico)
                .GreaterThanOrEqualTo(0).WithMessage("O Stock Crítico não pode ser negativo.")
                .LessThan(x => x.StockMinimo).WithMessage("O Stock Crítico tem de ser estritamente menor que o Stock Mínimo.");
        }
    }
}